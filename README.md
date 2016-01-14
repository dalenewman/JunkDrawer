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

### Configuration

Junk Drawer refers to files as junk, and the 
database as a drawer.  The file is an input, and 
the database is an output; both are connections. 
Open Junk Drawer's default configuration file 
*default.xml*.

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

The file is `*.*`, but this is changed at run-time. 
 The database is `Junk` on my local SQL Server.

### Get a File

The file must be Excel (e.g. `.xls`, `.xlsx`), or a delimited 
text file (e.g. `.csv`, `.txt`).

I searched Google for `filetype:csv colors` and found [colors.csv](https://github.com/codebrainz/color-names/blob/master/output/colors.csv).  Here's a sample:

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

SELECT TOP 10 * FROM colors;
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

When we see *colors.csv* above, it's easy for us
to notice the first row is a header, and subsequent 
lines are records.

Because there are few columns and records,
we can see a _comma_ delimits the values. 
Moreover, we see `Code` , `Name`, and `Hex` 
are text, and `Red`, `Green`, and `Blue` are numeric.

Junk Drawer has to see what we see:

1. the delimiter
2. the column names (if available)
3. the column data types

For excel files, the delimiter isn't necessary.

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
If you want to search for others, configure them in the input connection like this:

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

Now we must determine if the first record is column names, or data. 
So, we split it by the winning delimiter and tested:

* Are there duplicate values?
* Are there empty values?
* Are there white space values?
* Are there numbers?
* Are there dates?

If the answer is *Yes* to any question above, 
the first line cannot be column names.
If this happens, Excel-like column names are generated (i.e. A, B, C). 
In *colors.csv*, the first line answers _No_ to the 
questions, so it is used for column names.

### Data Types

Initially, every field is a considered to be a `string`. 
Often, when you're importing a file just to run some ad-hoc 
queries, strings are fine.  However, if you want to "type-check" 
the data, add types into your input connection like this: 

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

Types are checked in the order they appear.  So, be 
sure to add more restrictive data types first.  For example, 
a `byte` allows 0 to 255, and a `short` allows -32,768 to 32,767. 
If you test for `short` before you test for `byte`, all the `bytes` 
will end up as `shorts`.

Every value in the file is tested.  The first type where 
all the values are convertable wins. If none of the types 
allow all the values, a string is used.

A `string` is tested for length.  A field 
assumes the length of the longest value in the file (+1).  If you want 
control over string length, add `min-length` 
and/or `max-length` to the connection:

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

In the case above, I'm using [Autofac](http://autofac.org/) to wire up the 
`JunkImporter` dependencies.  A `JunkDrawer.Autofac` project is 
included in the solution.

### Options

#### Table Name
By default, Junk Drawer creates a view named after your 
file (without the extension).  For example, `colors.csv` is 
named `colors`. If you want to name your table something else, you can
set the `TableName` property in `JunkRequest`.

#### Configuration

You pass in the name of the file you want to import, and 
an optional configuration file. If you do not provide 
a configuration, *default.xml* is used.

You can make as many configurations as you want.  For example, 
let's say I wanted to import into SQLite instead of SQL Server. 
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

Now you can run this to import the file into SQLite:

`jd.exe c:\temp\colors.csv sqlite.xml`


### Precautions

#### Junk Overwrite

If you import the same file into Junk Drawer twice, it overwrites 
the previous table.  Don't worry though; it's only 
your junk.

#### Junk Overflow

I called it Junk Drawer because allowing folks to 
import files directly into a database can create a mess. 
You may want to keep an eye on it, or put your Junk database 
on an isolated test server where it can do no harm.

### Conclusion

Once in place, Junk Drawer can empower your trusted
friends to import their data into a Junk database
and run ad-hoc queries until their heart&#39;s content.

Of course there are going to be files that are so messed up,
that JunkDrawer won't be able to make any sense of them. In
that case, you'll have to resort to shouting, head-locks,
and noogies (aka the import wizard).