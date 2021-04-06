using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

using SimioAPI;
using SimioAPI.Extensions;

namespace RunExecutable
{

    public enum EnumArgLogic
    {
        None = 0,
        Delimited = 1,
        Python = 2
    }

    /// <summary>
    /// A Step to run an external executable (e.g. *.exe)
    /// The properties include the location of the interpreter, the python script
    /// and an RepeatingGroup to specify arguments for the python script.
    /// </summary>
    class RunExecutableDefinition : IStepDefinition
    {
        #region IStepDefinition Members

        /// <summary>
        /// Property returning the full name for this type of step. The name should contain no spaces.
        /// </summary>
        public string Name
        {
            get { return "RunExecutable"; }
        }

        /// <summary>
        /// Property returning a short description of what the step does.
        /// </summary>
        public string Description
        {
            get { return "Run an executable program with optional arguments"; }
        }

        /// <summary>
        /// Property returning an icon to display for the step in the UI.
        /// </summary>
        public System.Drawing.Image Icon
        {
            get { return null; }
        }

        /// <summary>
        /// Property returning a unique static GUID for the step.
        /// </summary>
        public Guid UniqueID
        {
            get { return MY_ID; }
        }
        static readonly Guid MY_ID = new Guid("{1C127142-2A55-45DD-B051-6ECC3ACD3692}"); //27Mar2021/DHouck

        /// <summary>
        /// Property returning the number of exits out of the step. Can return either 1 or 2.
        /// </summary>
        public int NumberOfExits
        {
            get { return 2; } // Alternate exit if errors
        }

        /// <summary>
        /// Method called that defines the property schema for the step.
        /// </summary>
        public void DefineSchema(IPropertyDefinitions schema)
        {
            try
            {

                // Example of how to add a property definition to the step.
                IPropertyDefinition pd;
                pd = schema.AddStringProperty("ExecutableLocation", "");
                pd.DisplayName = "Executable Location";
                pd.Description = "The full path to the executable program (.exe)";
                pd.Required = true;

                


                // A repeat group of columns and expression where the data will be written
                IRepeatGroupPropertyDefinition arguments = schema.AddRepeatGroupProperty("Arguments");
                arguments.Description = "The arguments for the executable. See Argument Logic for options";
                pd = arguments.PropertyDefinitions.AddExpressionProperty("ArgumentName", String.Empty);
                pd.Description = "The name of the argument";
                pd = arguments.PropertyDefinitions.AddExpressionProperty("Expression", String.Empty);
                pd.Description = "The Simio expression that will assigned to the argument.";

                pd = schema.AddBooleanProperty("WaitForExecutableToExit");
                pd.DisplayName = "Wait For Exit";
                pd.Description = "If true, then Step will wait in the Step until the executable completes its run";
                pd.Required = true;
                pd.SetDefaultString(schema, "False");

                IBooleanPropertyDefinition bpd = schema.AddBooleanProperty("CreateWindow");
                bpd.DisplayName = "Create Window";
                bpd.Description = "If True, the executable will appear in a new window. Useful for debugging";
                bpd.Required = true;
                bpd.SetDefaultString(schema, "True");

                IEnumPropertyDefinition epd = schema.AddEnumProperty("ArgumentLogic", typeof(EnumArgLogic));
                epd.Description = "Logic that describes how the Simio Arguments are created";
                epd.SetDefaultString(schema, "None");
                epd.Required = true;

                // Example of how to add a property definition to the step.
                pd = schema.AddStringProperty("Delimiter", "");
                pd.DisplayName = "Delimiter";
                pd.Description = "The delimiter to use around each argument when calling the executable";
                pd.Required = true;

                bpd = schema.AddBooleanProperty("UseShellExecute");
                bpd.DisplayName = "Use Shell Execute";
                bpd.Description = "If True, the executable will be run within a Windows Shell.";
                bpd.Required = true;
                bpd.SetDefaultString(schema, "True");

            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error creating RunExecutables Schema. Err={ex.Message}");
            }

        }

        /// <summary>
        /// Method called to create a new instance of this step type to place in a process.
        /// Returns an instance of the class implementing the IStep interface.
        /// </summary>
        public IStep CreateStep(IPropertyReaders properties)
        {
            return new RunExecutable(properties);
            
        }

        #endregion
    }

    class RunExecutable : IStep
    {
        IPropertyReaders _properties;
        IPropertyReader _prExecutableLocation;
        IRepeatingPropertyReader _prArguments;
        IPropertyReader _prWaitForExit;
        IPropertyReader _prCreateWindow;
        IPropertyReader _prArgLogic;
        IPropertyReader _prDelimiter;
        IPropertyReader _prUseShellExecute;


        string _executablePath;


        public RunExecutable(IPropertyReaders properties)
        {
            try
            {

                _properties = properties;
                _prExecutableLocation = _properties.GetProperty("ExecutableLocation");
                _prArguments = (IRepeatingPropertyReader)_properties.GetProperty("Arguments");
                _prWaitForExit = _properties.GetProperty("WaitForExecutableToExit");
                _prCreateWindow = _properties.GetProperty("CreateWindow");
                _prArgLogic = _properties.GetProperty("ArgumentLogic");
                _prDelimiter = _properties.GetProperty("Delimiter");
                _prUseShellExecute = _properties.GetProperty("UseShellExecute");

            }
            catch (Exception ex)
            {

                throw new ApplicationException($"RunExecutables. Perhaps a misnamed Property? Err={ex.Message}");
            }
        }

        #region IStep Members


        /// <summary>
        /// Get the arguments from the Repeating Group
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public List<Tuple<string, string>> GetArgumentsFromSimio(IStepExecutionContext context)
        {
            List<Tuple<string, string>> argList = new List<Tuple<string, string>>();
            string marker = "";

            int numArguments = _prArguments.GetCount(context);
            int ii = -1;

            try
            {
                // Get the values of all the arguments (repeating group)
                for (ii = 0; ii < numArguments; ii++)
                {
                    marker = $"Argument #{ii}";
                    using (IPropertyReaders argsRow = _prArguments.GetRow(ii, context))
                    {
                        IExpressionPropertyReader prExpression = argsRow.GetProperty("ArgumentName") as IExpressionPropertyReader;
                        string argName = prExpression.GetExpressionValue(context).ToString();

                        prExpression = argsRow.GetProperty("Expression") as IExpressionPropertyReader;

                        string exprVal = prExpression.GetExpressionValue(context).ToString();

                        marker = $"{marker}: Name={argName} Value={exprVal}";
                        argList.Add(new Tuple<string, string>(argName, exprVal));
                    }
                } // for

                return argList;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Marker={marker}. Processing argument {ii} of {numArguments}. Err={ex.Message}");
            }
        }





        /// <summary>
        /// Method called when a process token executes the step.
        /// </summary>
        public ExitType Execute(IStepExecutionContext context)
        {

            if ( string.IsNullOrEmpty(_executablePath) )
            {

                // executable location.
                _executablePath = _prExecutableLocation.GetStringValue(context);
                if (!File.Exists(_executablePath))
                {
                    Alert(context, $"Cannot find Executable at location={_executablePath}");
                    return ExitType.AlternateExit;
                }
            }

            List<Tuple<string, string>> argList = GetArgumentsFromSimio(context);
            string argLogic = _prArgLogic.GetStringValue(context);
            string delimiter = _prDelimiter.GetStringValue(context);

            string arguments = "";
            string workingFolder = "";

            
            switch (argLogic.ToUpper())
            {
                case "NONE":
                    {
                        PrepareArgumentsForNone(context, argList, out arguments);
                    }
                    break;
                case "DELIMITED":
                    {
                        PrepareArgumentsForDelimited(context, delimiter, argList, out arguments);
                    }
                    break;
                case "PYTHON":
                    {
                        PrepareArgumentsForPython(context, argList, out arguments, out workingFolder);
                    }
                    break;
            }


            bool createWindow = _prCreateWindow.GetDoubleValue(context) == 1;
            bool waitForExit = _prWaitForExit.GetDoubleValue(context) == 1;
            bool useShellExecute = _prUseShellExecute.GetDoubleValue(context) == 1;


            // create new process start info 
            ProcessStartInfo prcStartInfo = new ProcessStartInfo
            {
                // full path of the Python interpreter 'python.exe'
                FileName = _executablePath,
                Arguments = arguments,
                UseShellExecute = useShellExecute,
                RedirectStandardOutput = false, // Yeah, probably should do this one as well
                CreateNoWindow = !createWindow  // set to true if not debugging so the cmd doesn't flash up.
            };

            if ( !useShellExecute && workingFolder != null)
                prcStartInfo.WorkingDirectory = workingFolder;

            Trace(context, $"Starting Executable={_executablePath}: Args={prcStartInfo.Arguments}");

            try
            {
                using (Process process = Process.Start(prcStartInfo))
                {
                    if (waitForExit)
                        process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Alert(context, $"RunExecutable for={_executablePath} threw exception={ex.Message}");
                return ExitType.AlternateExit;
            }

            return ExitType.FirstExit;
        }

        /// <summary>
        /// Python wants the first arg to be the Python script, and
        /// all other args should be double-quoted (in case of embedded spaces)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="originalArgs"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        private static bool PrepareArgumentsForNone(IStepExecutionContext context, List<Tuple<string, string>> originalArgs, out string arguments)
        {
            List<string> argList = new List<string>();
            string marker = "";
            try
            {
                StringBuilder sbArgs = new StringBuilder();
                // Get the values of all the arguments (repeating group)
                // Get the values of all the arguments (repeating group)
                int ii = -1;
                foreach (Tuple<string, string> tt in originalArgs)
                {
                    ii++;
                    string exprVal = tt.Item2;
                    sbArgs.Append($@"{exprVal}");
                } // for

                arguments = sbArgs.ToString();
                return true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Marker={marker}. Err={ex}");
            }
        }

        /// <summary>
        /// Python wants the first arg to be the Python script, and
        /// all other args should be double-quoted (in case of embedded spaces)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="originalArgs"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        private static bool PrepareArgumentsForDelimited(IStepExecutionContext context,
            string delimiter, List<Tuple<string, string>> originalArgs,
            out string arguments)
        {
            List<string> argList = new List<string>();
            string marker = "";
            try
            {
                StringBuilder sbArgs = new StringBuilder();

                // Get the values of all the arguments (repeating group)
                // Get the values of all the arguments (repeating group)
                int ii = -1;
                foreach (Tuple<string, string> tt in originalArgs)
                {
                    ii++;
                    string exprVal = tt.Item2;
                    if (exprVal.Contains('"'))
                    {
                        exprVal = exprVal.Replace('"', '?');
                    }
                    sbArgs.Append($@" {delimiter}{exprVal}{delimiter}");
                } // for

                arguments = sbArgs.ToString();
                return true;
            }
            catch (Exception ex)
            {

                throw new ApplicationException($"Marker={marker}. Err={ex}");
            }
        }

        /// <summary>
        /// Python wants the first arg to be the Python script, and
        /// all other args should be double-quoted (in case of embedded spaces)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="originalArgs"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        private static bool PrepareArgumentsForPython(IStepExecutionContext context, List<Tuple<string, string>> originalArgs,
            out string arguments, out string workingFolder)
        {
            List<string> argList = new List<string>();
            workingFolder = "";

            string marker = "";
            try
            {
                if (originalArgs.Count <= 0)
                {
                    throw new ApplicationException($"No arguments. Python expects at least one argument which is the Python Script");
                }

                string pythonScriptPath = originalArgs[0].Item2;
                if (!File.Exists(pythonScriptPath))
                {
                    throw new ApplicationException($"Cannot locate Python script={pythonScriptPath}");
                }

                string folderPart = Path.GetDirectoryName(pythonScriptPath);
                string filePart = Path.GetFileName(pythonScriptPath);

                workingFolder = folderPart;

                StringBuilder sbArgs = new StringBuilder();

                // Get the values of all the arguments (repeating group)
                int ii = -1;
                foreach (Tuple<string, string> tt in originalArgs)
                {
                    ii++;
                    string exprVal = tt.Item2;
                    if (exprVal.Contains('"'))
                    {
                        exprVal = exprVal.Replace('"', '?');
                    }
                    sbArgs.Append($@" ""{exprVal}"" ");
                } // for

                arguments = sbArgs.ToString();


                return true;
            }
            catch (Exception ex)
            {

                throw new ApplicationException($"Marker={marker}. Err={ex}");
            }
        }


        /// <summary>
        /// Display a trace line (if Simio has Trace mode on)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        private void Trace(IStepExecutionContext context, string message)
        {
            context.ExecutionInformation.TraceInformation($"{message}");
        }

        /// <summary>
        /// Alert Simio to a problem
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        private void Alert(IStepExecutionContext context, string message)
        {
            context.ExecutionInformation.ReportError($"RunExecutable Step Error={message}");
        }

        #endregion
    }
}
