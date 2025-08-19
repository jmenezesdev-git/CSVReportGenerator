Writing a program that takes XML or tag structured files in the style of XML and converts them into .CSV files based on a template document.

Template document intended to allow for XPath references to enable accurate output generation
Will include custom attribute to make invoking common details easier
Might consider expanding to alternative format like JSON or something as well.

Base functionality complete. Program works with Examples provided.

Testing incomplete.

Arguments
-outputSchema: A required argument. Must point to a file that contains a valid xml structure to use as a formatting guide for report output. See Examples folder for reference.
-filter An optional argument specifying which files to include in the (optionally included) input directory. Uses regex defaults to "*.xml"
-input: An optional argument specifying file or folder paths to input files. Defaults to execution directory
-output: An optional argument specifying file output directory and/or file name. Defaults to report.csv and execution directory