JunkDrawer
==============
JunkDrawer imports an excel or delimited file into 
a database.  It is [open source](https://github.com/dalenewman/JunkDrawer) under
Apache 2.

### Introduction

**analyst**: "*Is there something that just
automatically imports files into a database?*"

**programmer**: "*No. You have to use the data import wizard.*"

The data analyst sighed as he recalled the wizard.

<img src="http://www.codeproject.com/KB/database/716239/SqlServerImportExportWizard.png" class="img-responsive img-thumbnail" alt="SQL Server Import Wizard" />

Using the wizard to import a text file into SQL Server 
goes something like this:

*   Install SQL Server Management Studio.
*   Find Tasks and choose Import Data.
*   Select &quot;Flat File Source.&quot;
*   Browse for the file.
*   Preview the data.
*   Specify the delimiter.
*   Specify if the first row is column names.
*   Preview the data (again).
*   Go to each column and choose the correct data type or use &quot;Suggest Types&quot; feature.
*   Choose if you want to save the SSIS package for later.
*   Execute it

**programmer**: &quot;*The wizard helps you import any kind of file.*&quot;

**analyst**: &quot;*That's great.
But, it would be better if a program just figured it out
for me.*&quot;

**programmer**: &quot;*Sorry. You have to use the wizard.
If you get an error message, fix the problem
and try again.*&quot;

**analyst**: &quot;*I get a lot of different files.
Using the wizard is repetitive.
This wastes my time.*&quot;

At this, the programmer shouted assembly language
and put the data analyst in a head lock.
Balling his fist, he pressed his knuckles hard against
the analyst's head and rubbed *back* and *forth*.

**programmer**: &quot;*You come to my cube, without a ticket,
complaining about YOUR time being wasted?*&quot;

---

Sadly, this scenario happens a lot in IT offices. Recently, 
while forcing a staff member to learn `SQL`,
he said:

**staff**: &quot;*SQL is great, but how do I get these files
into the database?*&quot;

I thought of the import wizard, but it didn't feel right. 
If he found out that he'd have to run the wizard every time 
and most likely deal with error messages, it would be a 
stumbling block for him. Also, I knew the conversation would most likely
end in a head-lock (as depicted above).

So, instead of giving him the beat down, I decided to create a program that 
makes it easier to import an Excel or text file into a database.

### Getting Started

Junk Drawer refers to files as junk, and the 
database as a drawer.  The file is an input, and 
the database is an output.  Both are connections. 

To configure the connections, open Junk Drawer's default 
configuration file *default.xml*.

```xml
<jd>
  <connections>
    <add name="input" 
         provider="file" 
         file="*.*" />
    <add name="output" 
         provider="sqlserver" 
         server="localhost" 
         database="Junk" />
  </connections>
</jd>
```

The input is set to a file.  The file `*.*` is changed to your 
file at run-time. The output is set to a local SQL Server database named Junk. 
For SQL Server, the default database connection uses trusted security. 
If you're using a native account, you may add a `user` and 
`password` attribute.  You may also use a `connection-string` attribute 
instead.

### Get a File

The file must be Excel (e.g. `.xls`, `.xlsx`), or a delimited 
text file (e.g. `.csv`, `.txt`).

I searched Google for `filetype:csv colors` and found [colors.csv](https://github.com/codebrainz/color-names/blob/master/output/colors.csv). You 
can find some pretty neat stuff on Google with the `filetype` term. 
Here's a sample of *colors.csv*:

```bash
Code,Name,Hex,Red,Green,Blue
air_force_blue_raf,"Air Force Blue (Raf)",#5d8aa8,93,138,168
air_force_blue_usaf,"Air Force Blue (Usaf)",#00308f,0,48,143
air_superiority_blue,"Air Superiority Blue",#72a0c1,114,160,193
alabama_crimson,"Alabama Crimson",#a32638,163,38,56
alice_blue,"Alice Blue",#f0f8ff,240,248,255
alizarin_crimson,"Alizarin Crimson",#e32636,227,38,54
alloy_orange,"Alloy Orange",#c46210,196,98,16
almond,"Almond",#efdecd,239,222,205
```

Junk Drawer (*jd.exe*) imports it from the command line 
like this:

`jd.exe c:\temp\colors.csv`

Now it can be queried:

```sql
USE Junk;

SELECT TOP 10 Code, Name, Hex, Red, Green, Blue
FROM colors;
```

```bash
Code                  Name                   Hex     Red Green Blue
--------------------- ---------------------- ------- --- ----- ----
air_force_blue_raf    Air Force Blue (Raf)   #5d8aa8 93  138   168
air_force_blue_usaf   Air Force Blue (Usaf)  #00308f 0   48    143
air_superiority_blue  Air Superiority Blue   #72a0c1 114 160   193
alabama_crimson       Alabama Crimson        #a32638 163 38    56
alice_blue            Alice Blue             #f0f8ff 240 248   255
alizarin_crimson      Alizarin Crimson       #e32636 227 38    54
alloy_orange          Alloy Orange           #c46210 196 98    16
almond                Almond                 #efdecd 239 222   205
amaranth              Amaranth               #e52b50 229 43    80
amber                 Amber                  #ffbf00 255 191   0
```

The resulting data structure is similar to this:

```sql
CREATE TABLE colors(
    Code NVARCHAR(40),
    Name NVARCHAR(42),
    Hex NVARCHAR(8),
    Red TINYINT,
    Green TINYINT,
    Blue TINYINT
);
```

### How Does it Work?

When we glance at *colors.csv* above, it's easy for us
to see the first row is a header, and subsequent 
rows are records.

Moreover, we see that a _comma_ delimits the values.  We 
also recognize patterns with the fields.  We see that `Code`, 
`Name`, and `Hex` are text, and `Red`, `Green`, and `Blue` 
are numeric.

Junk Drawer has to see the same thing as we do:

1. the delimiter
2. the column names (if available)
3. the column data types

### Finding the Delimiter
 
100 lines are examined for delimiters. If delimiters
are found, the average number per line and [standard
deviation](http://www.mathsisfun.com/data/standard-deviation.html)
is calculated.

Then, the delimiter with the lowest [coefficient
of variation](http://en.wikipedia.org/wiki/Coefficient_of_variation)
is declared winner.  This provides us with the most 
consistent delimiter across the first 100 records.

The default delimiters searched for are comma, pipe, tab, and semicolon. 
If you want control over the delimiters, configure them in 
the input connection like this:

```xml
<add name="input" provider="file" file="*.*">
    <delimiters>
        <add name="comma" character=","/>
        <add name="pipe" character="|"/>
        <add name="tab" character="&#009;"/>
        <add name="semicolon" character=";"/>
    </delimiters>
</add>
```

### Column Names

The first line is split by the winning delimiter 
and tested for:

* duplicates
* empties
* white space values
* numbers
* dates

If there are any of the above, the first line is not suitable 
for column names. Excel-like column names are generated (i.e. A, B, C) 
if necessary.  In *colors.csv*, the first line doesn't have any 
duplicates, empties, white space values, numbers, or dates, 
so it is used as column names.

### Data Types

Initially, every field is considered a `string`. 
Often, when importing a file for ad-hoc queries, 
strings are fine. However, if you want to *type-check* 
the data, add types into the input connection like this: 

```xml
<add name="input" provider="file" file="*.*">
    <types>
        <add type="bool"/>
        <add type="byte"/>
        <add type="short"/>
        <add type="int"/>
        <add type="long"/>
        <add type="single"/>
        <add type="double"/>
        <add type="decimal"/>
        <add type="datetime"/>
    </types>
</add>
```

Types are checked in the order they appear. Be sure to add 
more restrictive data types first.  For example, a `byte` 
allows 0 to 255, and a `short` allows -32,768 to 32,767. 
If you test for `short` first, and then for `byte`, all 
the *would-be* `bytes` end up as `shorts`.

Every value in a field is checked for type compatibility. 
The first type that provies compatible is used. If no type 
is compatible, a `string` is used.

A `string` is tested for length. A field assumes the length 
of the longest value in the file (+1). If you want control 
over string length, add `min-length` and/or `max-length` to 
the connection:

```xml
<add name="input" 
     provider="file" 
     file="*.*"
     min-length="64"
     max-length="4000" />
```

Once the values are type and/or length checked, Junk Drawer
tries to import the file

### In Code

JunkDrawer may be used in code like this:

```csharp
JunkResponse response;
var request = new JunkRequest(@"c:\temp\colors.csv", "default.xml");
using (var scope = new AutofacJunkBootstrapper(request)) {
    response = scope.Resolve<JunkImporter>().Import();
}

```
Just like the *jd.exe* executable, `JunkRequest` requires the 
file name you want to import, and a configuration.

In the case above, I'm using [Autofac](http://autofac.org/) to wire up the 
`JunkImporter` dependencies.  A `JunkDrawer.Autofac` project is 
included in the solution to demonstrate how `JunkImporter` is composed.

### Options

#### Table Name
By default, Junk Drawer creates a view named after your 
file (without the extension).  For example, `colors.csv` is 
named `colors`. If you want to name your table something else, 
set the `TableName` property in `JunkRequest`.

#### Configuration

If you do not provide a configuration, *default.xml* is used.

The configuration is file based.  You may make as 
many configurations as you want.  For example, 
if I wanted to import into SQLite instead of SQL Server. 
I could create *sqlite.xml* like this:

```xml
<jd>
    <connections>
        <add name="input" provider="file" file="*.*" />
        <add name="output"
             provider="sqlite"
             file="c:\temp\junk.sqlite3" />
    </connections>
</jd>
```

Now you can import *colors.csv* into SQLite:

`jd.exe c:\temp\colors.csv sqlite.xml`

Once imported, you may use something like 
[DB Browser for SQLite](http://sqlitebrowser.org/) to query it.

### Precautions

#### Junk Overwrite

If you import the same file into Junk Drawer twice, it overwrites 
the previous table.  Don't worry though; it's only 
your junk.

#### Junk Overflow

I called it Junk Drawer because allowing folks to 
import files directly into a database can create a mess. 
You may want to keep an eye on it, or put your Junk database 
on an isolated test server where it can't hurt anybody.

### Conclusion

Once in place, Junk Drawer can empower your trusted
friends to import their data into a Junk database
and run ad-hoc queries until their heart's content.

Of course, there are files that are so messed up that 
JunkDrawer won't be able import them. In that case, 
you'll have to resort to shouting, head-locks,
and noogies (aka the import wizard).

### Credits

Junk Drawer is not possible without:

* [Microsoft .NET](https://www.microsoft.com/net)
* [Cfg-Net](https://github.com/dalenewman/Cfg-NET) - Apache 2
* [AutoFac](http://autofac.org/) - MIT
* [Dapper](https://github.com/StackExchange/dapper-dot-net) - Apache 2
* [System.Data.SQLite](https://system.data.sqlite.org)
* [Npgsql](http://www.npgsql.org/)
* [MySql.Data](http://dev.mysql.com/downloads/connector/net/)- GPL 2
* [FileHelpers](http://www.filehelpers.net/) - MIT
* [ExcelDataReader](https://github.com/ExcelDataReader/ExcelDataReader) - MIT
* [SharpZipLib](https://icsharpcode.github.io/SharpZipLib/) - GNU
* [Nlog](http://nlog-project.org/) - BSD


