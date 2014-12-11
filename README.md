JunkDrawer
==============
Making Excel and file import to a database a one step process.

Released under GNU General Public License, version 3 (GPL-3.0).

##CodeProject Article

###Introduction

**analyst**: &quot;_Is there something that just automatically imports files into a database?_&quot;

**nerd**: &quot;_No. Use the data import wizard._&quot;

The data analyst sighed.. remembering the wizard...

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

**nerd**: _&quot;The wizard helps you import any kind of file._&quot;

**analyst**: _&quot;That&#39;s great. But, it would be better if I didn&#39;t have to answer so many questions and the program just analyzed and imported the file for me._&quot;

**nerd**: _&quot;You&#39;re dreaming. You have to use the wizard. Go back to your cube. Press the keys. Click the mouse. Do this until the Wizard says it successfully imported the file. If you get the fail message, you did something wrong. Respond to the error message and go back and fix it.&quot;_

**analyst**: _&quot;I get a lot of slightly different files. Using the wizard is repetitive. This wastes a lot of my time...&quot;_

At this, the nerd put the data analyst in a head lock. Balling up his fist, he pressed his knuckles hard against the analyst&#39;s head; rubbing _back_ and _forth_, causing a great deal of painful friction.

**nerd**: _&quot;You come to my cube, without a ticket, complaining about YOUR time being wasted?&quot;_

The nerd pushed the analyst away from him. As the analyst fell forward, the nerd Tae Kwon Doe&#39;d him squarely in the buttocks.

**nerd**: _&quot;Now get out of here!&quot;_

Humiliated, and unable to match the nerd&#39;s mightiness, the data analysts sulked back to his cubicle.

---

Sadly, this scenario happens a lot in IT offices.

Just recently, while forcing an IT staff member to learn `SQL`, he asked me something similar:

**staff**: _&quot;Yeah, SQL is great and all, but how do I get files into the database so I can query them?&quot;_

I began to explain the data import wizard (as described above), but it didn&#39;t feel right. I felt bad that he&#39;d have to run the wizard every time he wanted to query data. I knew it would take him forever, and he probably wouldn't do it. Also, I knew the conversation would most likely end in a destructive battle (as depicted above). So, optimistically, I thought there might be a better way.

---

I always tell people, that if you can think of it, then chances are it already exists. You just have to Google for it. I should have followed my own advice, but I didn&#39;t. See, I&#39;m a programmer. I have these times where _all_ I want to do is program. I&#39;m not even a great programmer (as indicated by the comments below), but, I just love it, and I don&#39;t care what anyone thinks because it makes me happy, happy, happy.

So, I decided to bypass the Google search and create an open source project called [Junk Drawer](https://github.com/dalenewman/JunkDrawer "Junk Drawer on GitHub"). As it turns out, the name Junk Drawer is already used for a variety of things, but it&#39;s too late to change the name. Anyway, its goal is to make the majority of Excel or text file importing a one step, no-brainer process. It should also reduce the likelihood of terrible brawls between data analysts and nerds.

### Requirements

I don&#39;t want staff members to need a copy of Junk Drawer on their computer. I just want them to drop a file on a network share and have it imported. For myself, since I have Junk Drawer, I want to be able to right-click and use the _Open With_ option. For both requirements, all I need is a .NET Console application. I can enable the file monitoring and action trigger with job automation software (i.e. [Visual Cron](http://www.visualcron.com/ "Visual Cron")).

### A Demo

Before you run Junk Drawer for the first time, make sure you create a `Junk` database for all the files you&#39;re going to import. Then, put a SQL Server connection string in the configuration file _jd.exe.config_.

<pre class="prettyprint" lang="xml">
&lt;transformalize&gt;
    &lt;processes&gt;
        &lt;add name=&quot;JunkDrawer&quot;&gt;
            &lt;connections&gt;
                &lt;add name=&quot;output&quot; connection-string=&quot;connection-string&quot; /&gt;
            &lt;/connections&gt;
        &lt;/add&gt;
    &lt;/processes&gt;
&lt;/transformalize&gt;
</pre>

Here are the contents of a sample text file:

<pre class="prettyprint" lang="text">
Name,Birthday,Points
Dale,3/3/1981 9 AM,73
Tara,12/31/1990,1042
Grace,9/9/2000 11 PM,56
Gavin,7/3/2010,13
</pre>

To import it, just run:

<pre class="prettyprint" lang="text">
jd c:\sample.txt
</pre>

Junk Drawer (_jd.exe_) imports the file, and now I can go query it:

<pre class="prettyprint" lang="sql">
SELECT Name, Birthday, Points FROM sample;
</pre>
<pre class="prettyprint" lang="text">
Name  Birthday            Points
----- ------------------- -----------
Dale  1981-03-03 09:00:00 73
Gavin 2010-07-03 00:00:00 13
Tara  1990-12-31 00:00:00 1042
Grace 2000-09-09 23:00:00 56    </pre>

The table structure looks something like this:

<pre class="prettyprint" lang="sql">
CREATE TABLE sample(
    BirthDay DATETIME,
    Name NVARCHAR(5),
    Points INT
);
</pre>

### How Does it Work?

To be able to import a text file into a database, Junk Drawer (JD) has to figure out three things:

1. the delimiter
2. the column names (if available)
3. the column data types

For Excel files, you can skip the first step.

### Delimiters

First, a number of lines are loaded from the file. Then, popular delimiters are counted in each line. If the number of delimiters is greater than zero, and the same number of delimiters is found in each line, then that delimiter *may* be the delimiter. That is to say, if the same number of commas are found in each line, then odds are this is a comma delimited file.

### Column Names

To determine column names, the first row is split by the potential delimiter and tested. We don&#39;t know if it contains column names, or if it is merely the first record in the file. So, I run these tests:

* Are there any duplicate field values?
* Are there any empty values?
* Are there any white space values?
* Are there any numeric values?
* Are there any date time values?

If any of the answers are &quot;Yes,&quot; then the first line cannot be used as column names. If this happens, default column names are generated (i.e. A, B, C, etc. like Excel). In the example above; Name, Birthday, and Points answer &quot;No&quot; to all these questions and make good column names.

### Data Types

Rather than write new code to efficiently run through a file and check data types, I have elected to use a library called [Transformalize](https://github.com/dalenewman/Transformalize "Transformalize on GitHub"). The guy who wrote it is AWESOME! Well, he&#39;s not _that_ awesome, but it&#39;s based on [Rhino ETL](https://github.com/ayende/rhino-etl "Rhino ETL on GitHub"), which was created by [Ayende](https://github.com/ayende), who actually IS awesome! So, it inherits a degree of awesomeness.

Given that we know the file location, the delimiter, and the column names, I am able to &quot;configure&quot; (not code) Transformalize to load all the records and perform &quot;type conversion&quot; validation on every value.

If every value in the column passes a data type validation, then I use that data type for database storage. For example, if all values in a column respond &quot;Yes,&quot; to the question &quot;Are you an integer?,&quot; then I store it in an `INTEGER` data type. If there are mixed or failing validation results, I default to use `NVARCHAR(x)`. Anything goes in an `NVARCHAR `data type. It&#39;s for a variable number of Unicode characters, where `x` is set to the maximum length found in the column.

The data types are the final piece of information we need to import the file into a database. Again, instead of writing new code, I configure Transformalize to take care of importing the file into the database.

### The Code

A lot of Junk Drawer just uses Transformalize. If you&#39;re curious, you&#39;re welcome to explore the [source code](https://github.com/dalenewman/JunkDrawer/tree/master/JunkDrawer "The Source Code on GitHub"). More likely, the only time you&#39;ll want to look at the [source code](https://github.com/dalenewman/JunkDrawer/tree/master/JunkDrawer "the source code") is if you run Junk Drawer and it doesn&#39;t import your file right. Like, for example, I&#39;m only searching for commas, pipes, semi-colons, and tabs for delimiters... You might have files that are delimited by 6&#39;s or something, I don&#39;t know. So, you could create a fork on [GitHub](https://github.com/) and add &quot;6&quot; in the list of delimiters to check for, or better yet, make it configurable!

### Conclusion

Well that&#39;s the Junk Drawer in a nut-shell. I called it Junk Drawer because importing files directly into a database can get messy. It ends up an uncontrolled staging area for data. Nevertheless, it empowers your trusted friends to put some data in there and run ad-hoc queries until their heart&#39;s content.

Now if you&#39;re the acting DBA, and you put your `Junk` on an important server, make sure you have monitors in place for disk space, CPU abuse, and excessive resource blocking. You should have this all setup regardless if you have Junk Drawer running, but especially if you do. Remember to control access to both your network share and your Junk database with Active Directory groups. :-)

### Updates

### More Configuration

Since the original article, I added more configuration.  I also moved the configuration into my [Transformalize](http://www.transformalize.com) library (which was already a dependency). Here&#39;s a sample:

<pre class="prettyprint" lang="xml">
  &lt;transformalize&gt;
    &lt;processes&gt;
      &lt;add name=&quot;JunkDrawer&quot;&gt;
        
        &lt;connections&gt;
          &lt;add name=&quot;output&quot; server=&quot;localhost&quot; database=&quot;Junk&quot; user=&quot;&quot; password=&quot;&quot;/&gt;
        &lt;/connections&gt;

        &lt;file-inspection sample=&quot;100&quot; min-length=&quot;64&quot; max-length=&quot;1024&quot;&gt;
          &lt;types ignore-empty=&quot;true&quot;&gt;
            &lt;add type=&quot;boolean&quot;/&gt;
            &lt;add type=&quot;byte&quot;/&gt;
            &lt;add type=&quot;int16&quot;/&gt;
            &lt;add type=&quot;int32&quot;/&gt;
            &lt;add type=&quot;int64&quot;/&gt;
            &lt;add type=&quot;single&quot;/&gt;
            &lt;add type=&quot;double&quot;/&gt;
            &lt;add type=&quot;decimal&quot;/&gt;
            &lt;add type=&quot;datetime&quot;/&gt;
          &lt;/types&gt;
          &lt;delimiters&gt;
            &lt;add name=&quot;comma&quot; character=&quot;,&quot;/&gt;
            &lt;add name=&quot;pipe&quot; character=&quot;|&quot;/&gt;
            &lt;add name=&quot;tab&quot; character=&quot;&amp;#009;&quot;/&gt;
            &lt;add name=&quot;semicolon&quot; character=&quot;;&quot;/&gt;
          &lt;/delimiters&gt;
        &lt;/file-inspection&gt;

      &lt;/add&gt;

    &lt;/processes&gt;
  &lt;/transformalize&gt;
</pre>

The `types` collection gives you control over what data types are checked for. You may remove them all. If you do, every field will assume the default type (e.g. `string`). This can be useful it you don&#39;t care about types, or are troubleshooting.

The `delimiters` collection gives you control over what delimiters are checked for. Currently, only single characters are supported.

The `sample` attribute allows you to reduce the amount of data type inspection. The lower the percentage, the fewer records get checked. You may set this to 100, or remove it altogether if you want to check everything (the original behavior).

### Bug Fixes

+ 2014-??-?? Handle .csv format correctly, allowing for commas within double-quotes.
+ 2014-05-28 Handle .xls (older binary excel files) correctly.
+ 2014-12-10 Move some features into Transformalize and also handle empty, or single row files.

