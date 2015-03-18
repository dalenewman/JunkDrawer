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
Balling up his fist, he pressed his knuckles against
the analyst&#39;s head; rubbing _back_ and _forth_.

**programmer**: _&quot;You come to my cube, without a ticket,
complaining about YOUR time being wasted?&quot;_

---

Sadly, this scenario happens a lot in IT offices.

Just recently, while forcing an staff member to learn `SQL`,
he asked me something similar:

**staff**: _&quot;SQL is great, but how do I get these files
into the database?&quot;_

I began to explain the data import wizard,
but it didn&#39;t feel right. I felt bad that he&#39;d have
to run the wizard every time he wanted to query data.
I knew it would take him forever, and he'd probably give up.
Also, I knew the conversation would most likely
end in a head-lock (as depicted above).
So, instead of giving him the beat down, I tried to think
of another way.

---

I tell people that if you can think of it,
then it already exists. You just have to Google it.
I should have followed my own advice, but I didn&#39;t. See,
I&#39;m a programmer. I have these times where _all_ I want to do
is program. I&#39;m not even a great programmer (as indicated by
the comments below), but I just love it, and I don&#39;t care what
anyone thinks because it makes me _happy, happy, happy_.

So, I decided to bypass Google and create an open source project
called [Junk Drawer](https://github.com/dalenewman/JunkDrawer "Junk Drawer on GitHub").
The goal is to make importing an Excel
or text file to a database easier.

### Requirements

I don&#39;t want staff members to need a copy of Junk Drawer
on their computer. I just want them to drop a file on a network
share and have it imported. For myself, I want to be able to 
right-click on a file and use the _Open With_ (Junk Drawer) option.

For both requirements, all I need is a .NET Console application.
For the network share, I can enable folder monitoring and 
an action trigger to execute Junk Drawer with job automation software
(i.e. [Visual Cron](http://www.visualcron.com/ "Visual Cron")).

### Configuration

Before you run Junk Drawer for the first time,
make sure you create a `Junk` database for all the
files you&#39;re going to import. Then, update
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

Now, in order for Junk Drawer to work, the file must be either
an Excel file, or some kind of delimited text file.

Here's a sample text file:

<pre class="prettyprint" lang="text">
<strong>Name,Birthday,Points</strong>
Dale<strong>,</strong>3/3/1981 9 AM<strong>,</strong>73
Tara<strong>,</strong>12/31/1990<strong>,</strong>1042
Grace<strong>,</strong>9/9/2000 11 PM<strong>,</strong>56
Gavin<strong>,</strong>7/3/2010<strong>,</strong>13
</pre>

If we save this (above) in a file called _sample.txt_, 
we could import it from the command line like this:

<pre class="prettyprint" lang="shell">
jd.exe c:\sample.txt
</pre>

Junk Drawer (_jd.exe_) imports the file, and now it can be queried:

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
Grace 2000-09-09 23:00:00 56    </pre>

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

Because there are only a few columns (3), and a few records (4), 
it's easy for us to see that the _comma_ is delimiting the values 
in each record.  Moreover, we can see `Name` is text, `Birthday` 
is a date, and `Points` is numeric.

Junk Drawer just has to do the same thing as we do. 
It has to figure out three things:

1. the delimiter
2. the column names (if available)
3. the column data types

For Excel files, you can skip the first step.

### Finding the Delimiter

By default, 100 lines are examined. A set of pre-defined 
delimiters are counted in each line. If any delimiters 
are found, the average per line and [standard 
deviation](http://www.mathsisfun.com/data/standard-deviation.html) 
are calculated. 

Then, the delimiter with the lowest [coefficient 
of variation](http://en.wikipedia.org/wiki/Coefficient_of_variation) 
is declared the winner.

I'm no statitician, but from what I gather, taking the delimiter 
with the lowest coefficient of variation gives us the most 
consistent delimiter across all lines.

### Column Names

We don&#39;t know if the first record is column names, or 
just another record. So, it is split by the 
winning delimiter and run through a series of tests:

* Are there any duplicate values?
* Are there any empty values?
* Are there any white space values?
* Are there any numbers?
* Are there any dates?

If any of the answers are &quot;Yes,&quot; 
then the first line cannot be used as column names. 
If this happens, default column names are 
generated (i.e. A, B, C, etc. like Excel). In the 
example above; `Name`, `Birthday`, and `Points` 
answer &quot;No&quot; to all the questions, so 
they make good column names.

### Data Types

By default, the entire file is run through data type validation 
against a set of types in a particular order. This produces a 
compatible set of data types, but may take some time depending 
on the size of your file. Here is the _default.xml_ configuration:

<pre class="prettyprint" lang="xml">
&lt;junk-drawer&gt;
  &lt;!-- connections --&gt;
  &lt;file-inspection&gt;
    &lt;add name=&quot;default&quot;
         min-length=&quot;64&quot;
         max-length=&quot;4000&quot;
         <strong>sample=&quot;100&quot;</strong>
         line-limit=&quot;100&quot;&gt;
      &lt;!-- the pre-defined set of types --&gt;
      <strong>&lt;types&gt;
        &lt;add type=&quot;boolean&quot;/&gt;
        &lt;!--&lt;add type=&quot;byte&quot;/&gt;--&gt;
        &lt;!--&lt;add type=&quot;int16&quot;/&gt;--&gt;
        &lt;add type=&quot;int32&quot;/&gt;
        &lt;add type=&quot;int64&quot;/&gt;
        &lt;!--&lt;add type=&quot;single&quot;/&gt;--&gt;
        &lt;!--&lt;add type=&quot;double&quot;/&gt;--&gt;
        &lt;add type=&quot;decimal&quot;/&gt;
        &lt;add type=&quot;datetime&quot;/&gt;
      &lt;/types&gt;</strong>
      &lt;!-- the pre-defined set of delimiters --&gt;
      &lt;delimiters&gt;
        &lt;add name=&quot;comma&quot; character=&quot;,&quot;/&gt;
        &lt;add name=&quot;pipe&quot; character=&quot;|&quot;/&gt;
        &lt;add name=&quot;tab&quot; character=&quot;&amp;#009;&quot;/&gt;
      &lt;/delimiters&gt;
    &lt;/add&gt;
  &lt;/file-inspection&gt;
&lt;/junk-drawer&gt;
</pre>

For example, if all the `Points` values in the file respond 
&quot;Yes,&quot; to the question &quot;Are you an integer?,&quot; 
then I store them in an `INTEGER` in the database. 
If there are mixed or failing validation results, like a 
combination of numbers and strings, I default to 
a string, which is then stored as an `NVARCHAR(x)` in the database. 

The data types are the final piece of information 
we need to import the file into a database.

### Doing it with Code

JunkDrawer comes with a library and an executable.  If you want to use 
the library, you can reference _JunkDrawer.dll_ (and [Transformalize.dll](https://github.com/dalenewman/Transformalize)) 
and run it like this:

<pre class="prettyprint" lang="cs">
    var cfg = new JunkCfg(File.ReadAllText(@&quot;default.xml&quot;));
    var request = new Request(@&quot;sample.txt&quot;, cfg);
    var response = new JunkImporter().Import(request);

    Console.WriteLine(&quot;Table: {0}&quot;, response.TableName);
    Console.WriteLine(&quot;Records: {0}&quot;, response.Records);
</pre>

This should produce output like this:

<pre class="prettyprint" lang="shell">
Table: TflAuto3757101240000
Records: 4
</pre>

If you want to name your table something specific, you can 
pass `TableName` in with the `Request`.

The default configuration comes in the file _default.xml_. 
However, you can make as many different configurations as 
you want, and pass them in as the second argument 
of the jd.exe executable (i.e. `jd sample.txt other.xml`).

### Conclusion

I called it Junk Drawer because importing files directly 
into a database can get messy. It ends up an uncontrolled 
staging area for data. Nevertheless, it empowers your trusted 
friends to put some data in there and run ad-hoc queries 
until their heart&#39;s content.

If you&#39;re the acting DBA, and you put your `Junk` on an 
important server, make sure you have monitors in place 
for disk space, CPU abuse, and excessive resource blocking. 
You should have this all setup regardless if you have Junk 
Drawer running.