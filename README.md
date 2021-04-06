# RunExecutableStep
A Simio user-defined Step to run an executable (.exe file) with arguments created from Simio expressions.

This includes both Executables and Source folders, as well as test programs and Documentation.

For a quick start, 
* create a Test\TestRunExecutable folder on your c: drive 
* move the exe files from Executables to your test folder
* move the dll file in Executables to your Documents\SimioUserExtensions folder
* and then run the test Simio project (.SPFX) in the Models folder.

When you run the Simio project, .txt files generated from the example executable files.
You can examine them to see how the Simio arguments were formatted.

The test Simio Project has three instances of this RunExecutable Step. One of them runs a Python Script and the other two run a .NET executable.

Note: to run the Step that invokes Python you will need a Python interpreter on your system. If you are not interested in running Python, simply delete the Python Step from the example Simio project.
