# Autoinjestion-dotnet
.NET Wrapper for Itunes Connect reporting service

Apple provides a Java library called [Autoinjestion](http://www.davidsweetman.com/www.apple.com/itunesnews/docs/Autoingestion.class.zip) for pulling down app download information from iTunes Connect.

You would run the Autojest class from the command line with a set of parmeters and a .csv file will be generated.  This can be a little awkward to use and I created a simple C# class that calls the same web service as the Autoinjest class.  This would allow to you to generate your own output, filter by app name, make repeated calls to get custom sets of data.  It gives you more control and easier access to your data.

Apple provides documentation for Autoinjest.class in the online [iTunes Connect Sales and Trends Guide: App Store ](https://www.apple.com/itunesnews/docs/AppStoreReportingInstructions.pdf) document.

The .NET wrapper provides a class named Autoinjestion with methods for requesting the reports.  The reports return a list of [AppleiTunesSalesReport](https://github.com/anotherlab/Autoinjestion-dotnet/blob/master/Autoinjestion/AppleiTunesSalesReport.cs) and the user can create reports from that list, filter by app name, and perform aggregate operations like summing up the number of new installs by a date range.