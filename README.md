# bloodpressureuploader
A windows command line tool to bulk upload blood pressure readings to online tools like MyChart®

Nothing special here's it's just a vb.net cli app that parses a CSV with blood pressure readings and submits it to my HMO's MyChart®. 
I am in no way affiliated with MyChart® is a registered trademark of Epic Systems Corporation, a Wisconsin-based entity. 
I am making this application available as-is, under an MIT license. 

If you are getting readings out of range, dial 911. 

How to use this?

Log into your "Track My Health" portal (at least that is what Kaiser Permanente calls it) and note the long URL, which includes a variable named episodeId.

Edit appsettings.sample.json and save it as as appsettings.json. 

You can use bp_readings.sample.csv as a template for your readings data.

When you build the project in Visual Studio you will get a separate Chrome session, which will send you to your HMO's login page. Once logged-in, go back to the terminal that opened when you started the build and hit enter, this will signal the code that it is ready to navigate to the input furm and do its thing.

