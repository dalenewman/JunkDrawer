JunkDrawer
==============
JunkDrawer is a tool that imports excel or delimited files
into a database.  It is [open source](https://github.com/dalenewman/JunkDrawer) under
GNU General Public License, version 3 (GPL-3.0).

###Introduction

**analyst**: &quot;_Is there something that just
automatically imports files into a database?_&quot;

**programmer**: &quot;_No. You have to use the data import wizard._&quot;

The data analyst sighed as he recalled the wizard.

<img src="http://www.codeproject.com/KB/database/716239/SqlServerImportExportWizard.png" class="img-responsive img-thumbnail" alt="SQL Server Import Wizard" />

Using the wizard to import a text file goes something like this:

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

**programmer**: _&quot;The wizard helps you import any kind of file._&quot;

**analyst**: _&quot;That&#39;s great.
But, it would be better if I didn&#39;t have to answer
so many questions and the program just figured it out
and imported the file.&quot;_

**programmer**: _&quot;Sorry. You have to follow the wizard.
If you get an error message, fix the problem
and try again.&quot;_

**analyst**: _&quot;I get a lot of different files.
Using the wizard is repetitive.
This wastes my time.&quot;_

At this, the programmer started shouting assembly language
and put the data analyst in a head lock.
Balling up his fist, he pressed his knuckles hard against
the analyst's head and rubbed _back_ and _forth_.

**programmer**: _"You come to my cube, without a ticket,
complaining about YOUR time being wasted?"_

---

Sadly, this scenario happens a lot in IT offices.

Just recently, while forcing an staff member to learn `SQL`,
he asked me something similar:

**staff**: _"SQL is great, but how do I get these files
into the database?"_

I began to explain the data import wizard,
but it didn't feel right. I felt bad that he'd have
to run the wizard every time he wanted to query data.
I knew it would take him forever, and he'd probably give up.
Also, I knew the conversation would most likely
end in a head-lock (as depicted above).
So, instead of giving him the beat down, I tried to think
of another way.

---

I tell people that if you can think of it,
then it already exists. You just have to Google it.
I should have followed my own advice, but I didn't. See,
I'm a programmer. I have these times where _all_ I want to do
is program. I'm not even a great programmer (as indicated by
the comments below), but I just love it, and I don't care what
anyone thinks because it makes me _happy, happy, happy_.

So, I decided to bypass Google and create an open source project
called [Junk Drawer](https://github.com/dalenewman/JunkDrawer "Junk Drawer on GitHub").
The goal is to make importing an Excel
or text file to a database easier.

### Configuration

Before you run Junk Drawer for the first time,
make sure you create a `Junk` database for all the
files you're going to import. Then, update
Junk Drawer's configuration file _default.xml_.

<pre class="prettyprint" lang="xml">
&lt;junk-drawer&gt;
  &lt;connections&gt;
    &lt;add name=&quot;output&quot; 
         provider=&quot;sqlserver&quot; 
        <strong>server</strong>=&quot;localhost&quot; 
        <strong>database</strong>=&quot;Junk&quot; /&gt;
  &lt;/connections&gt;
  &lt;!-- more later --&gt;
&lt;/junk-drawer&gt;
</pre>

**Note**: For a connection, you may use the `server`
and `database` attributes, or one called `connection-string`.
This lets Junk Drawer know where you want to keep your junk.

### Get a File

In order for Junk Drawer to work, the file must be either
an Excel file, or some a delimited text file.

Here's a sample text file:

<pre class="prettyprint" lang="text">
<strong>Name,Birthday,Points</strong>
Dale<strong>,</strong>3/3/1981 9 AM<strong>,</strong>73
Tara<strong>,</strong>12/31/1990<strong>,</strong>1042
Grace<strong>,</strong>9/9/2000 11 PM<strong>,</strong>56
Gavin<strong>,</strong>7/3/2010<strong>,</strong>13
</pre>

If we save this in a file called _sample.txt_,
we could import it from the command line like this:

<pre class="prettyprint" lang="shell">
jd.exe c:\sample.txt
</pre>

Junk Drawer (_jd.exe_) imports the file.  Now it can be queried:

<pre class="prettyprint" lang="sql">
USE Junk

SELECT Name, Birthday, Points 
FROM sample;
</pre>

<pre class="prettyprint" lang="text">
Name  Birthday            Points
----- ------------------- -----------
Dale  1981-03-03 09:00:00 73
Gavin 2010-07-03 00:00:00 13
Tara  1990-12-31 00:00:00 1042
Grace 2000-09-09 23:00:00 56
</pre>

The table structure looks similar to this:

<pre class="prettyprint" lang="sql">
CREATE TABLE sample(
    BirthDay DATETIME,
    Name NVARCHAR(5),
    Points INT
);
</pre>

### How Does it Work?

When we see sample text above, it's easy for us
to notice the first row is different. We recognize it
as a set of column names. The lines that follow are records.

Because there are only a few columns and records,
it's easy for us to see a _comma_ is delimiting the values
in each record.  Moreover, we see `Name` is text, `Birthday`
is a date, and `Points` is numeric.

Junk Drawer has to do the same thing. It has to figure 
out three things:

1. the delimiter
2. the column names (if available)
3. the column data types

For excel files, the first step is skipped.

### Finding the Delimiter
 
By default, 100 lines are examined. A set of pre-defined
delimiters are counted in each line. If any delimiters
are found, the average per line and [standard
deviation](http://www.mathsisfun.com/data/standard-deviation.html)
are calculated.

Then, the delimiter with the lowest [coefficient
of variation](http://en.wikipedia.org/wiki/Coefficient_of_variation)
is declared the winner.

I'm no statistician, but I gather taking the delimiter
with the lowest coefficient of variation provides the most
consistent delimiter in the records.

### Column Names

We don't know if the first record is column names, or
just another record. So, it is split by the
winning delimiter and run through a series of tests:

* Are there any duplicate values?
* Are there any empty values?
* Are there any white space values?
* Are there any numbers?
* Are there any dates?

If any of the answers are _Yes_, 
the first line cannot be used as column names.
If this happens, default column names are
generated (i.e. A, B, C, etc. like Excel). In the
example above; `Name`, `Birthday`, and `Points`
answer _No_ to all the questions, so
they make good column names.

### Data Types

By default, 100% of the lines are tested against
a set of types defined in _default.xml_.

<pre class="prettyprint" lang="xml">
&lt;junk-drawer&gt;
  &lt;!-- connections --&gt;
  &lt;file-inspection&gt;
    &lt;add name=&quot;default&quot; <strong>sample=&quot;100&quot;</strong>&gt;
      &lt;!-- the pre-defined set of types --&gt;
        <strong>&lt;types&gt;
        &lt;add type=&quot;boolean&quot;/&gt;
        &lt;add type=&quot;int32&quot;/&gt;
        &lt;add type=&quot;int64&quot;/&gt;
        &lt;add type=&quot;decimal&quot;/&gt;
        &lt;add type=&quot;datetime&quot;/&gt;
      &lt;/types&gt;</strong>
      &lt;!-- delimiters --&gt;
    &lt;/add&gt;
  &lt;/file-inspection&gt;
&lt;/junk-drawer&gt;
</pre>

Take `Points` for example:

* Is 73 a `boolean`?  No
* Is 73 an `int32`? **Yes**
* Is 13 an `int32`? **Yes**
* Is 1042 an `int32`? **Yes**
* Is 56 an `int32`? **Yes**

So, `Points` is compatible with an `int32`, and it is stored as
an `INTEGER` in the database.

If there are mixed or failing validation results, like a
combination of numbers and strings, I default to
a string, which is then stored as an `NVARCHAR(x)` in the database.

If you want to increase the speed of data type
validation, at the cost of accuracy, change the 
`sample` size to something less than 100 (100%).

Also, the less data types you check for, the faster 
it runs.  For example, if you remove _ALL_ the types 
from the configuration, everything is stored as a string, 
and sometimes that's all you need.

Once the values are checked, Junk Drawer
has a compatible set of data types and it's ready 
to try and import the file

### In Code

JunkDrawer is a library _and_ an executable.

If you want to use the library, 
it's available on [Nuget](https://www.nuget.org/packages/JunkDrawer/). 

<pre class="prettyprint" lang="bash">
PM> Install-Package <strong>JunkDrawer</strong>
</pre>

Installing it brings down [Transformalize](https://github.com/dalenewman/Transformalize) 
and [Cfg-Net](https://github.com/dalenewman/Cfg-NET) as well. 
Here's a console application example:

<pre class="prettyprint" lang="cs">
using System;
using System.IO;
<strong>using JunkDrawer;
using Transformalize.Logging;</strong>

namespace ConsoleApplication1 {

    class Program {

        static void Main(string[] args) {

        var cfg = new JunkCfg(File.ReadAllText("default.xml"));
        if (!cfg.Problems().Any()) {
            var logger = new NullLogger();
            var request = new Request("sample.txt", cfg, logger);
            var response = new JunkImporter(logger).Import(request);

            Console.WriteLine("Table: {0}", response.TableName);
            Console.WriteLine("Records: {0}", response.Records);
        } else {
            foreach (var problem in cfg.Problems()) {
                Console.Error.WriteLine(problem);
            }
        }
        Console.ReadKey();

        }
    }
}
</pre>

Using [default.xml](https://github.com/dalenewman/JunkDrawer/blob/master/JunkDrawer.Console/default.xml) and 
[sample.txt](https://github.com/dalenewman/JunkDrawer/blob/master/Test/sample.txt), you should 
see output like this:

<pre class="prettyprint" lang="shell">
Table: sample
Records: 4
</pre>

#### Table Name
If you want to name your table something else, you can
set the `TableName` property in the `Request` object.

#### Configuration
The `Request` requires a 'Cfg-NET' configuration. I used _default.xml_ above. 
However, you can make as many different configurations as you want.

The executable can also use other configurations.  Just pass the 
configuration file name in as the second argument:

<pre class="prettyprint" lang="shell">
jd.exe sample.txt <strong>other.xml</strong>
</pre>

#### Logging
You have to pass a logger in.  There is a `NullLogger` in 
Transformalize, or a `ConsoleLogger` in JunkDrawer you can use, or 
you can implement Transformalize's `ILogger` and use your favorite 
logging library.  The executable implement's one using [Serilog](https://github.com/serilog/serilog).

### Precautions

####Overwriting Tables
If you import the same file into Junk Drawer twice, it will 
overwrite the table.  So, if you run `jd.exe` _sample.txt_ and it 
creates a `sample` table, then you run `jd.exe` _sample.txt_ again, it 
will over-write the `sample` table. Don't worry though; it's only 
your junk.

####Things Might Get Crazy
I called it Junk Drawer because importing files directly
into a database can get messy. It may end up an uncontrolled
staging area for data. If you&#39;re the acting DBA, and you put your `Junk` on an
important server, make sure you have monitors in place
for disk space, CPU abuse, and excessive resource blocking.

###Conclusion

Once in place, Junk Drawer can empower your trusted
friends to import their data into a Junk database
and run ad-hoc queries until their heart&#39;s content.

Of course there are going to be files that are so messed up,
that JunkDrawer won't be able to make any sense of them. In
that case, you'll have to resort to shouting, head-locks,
and noogies.