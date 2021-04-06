import sys
import datetime

# Sample program to be initiated by the Simio Step RunExecutable with "Python" ArgumentLogic.
# This runs python scripts with argument convention of: 1st arg is the script name, followed
# by arguments. All args are surrounded with a double-quote.
# The script append-prints the arguments it finds and redirects to a file.

def logit( message ):
    dt = datetime.datetime.now()
    print(dt.strftime("[%H:%M:%S.%f] "), message)  

# redirect stdout to a file

from contextlib import redirect_stdout

try:

    with open('c:\\test\\testRunExecutable\PythonScriptTakingArgumentsOutput.txt', 'a') as f:
        with redirect_stdout(f):

            logit('Name of the script: ' + sys.argv[0])

            numArgs = len(sys.argv)

            logit('Number of arguments: ' + str(numArgs))
    
            for arg in range(0,numArgs):
                logit("Arg[" + str(arg) + "]=" + sys.argv[arg] )
        
            logit('The list of arguments: ' + str(sys.argv))
except:
    e = sys.exc_info()[0]
    print("Error= %s" % e)
