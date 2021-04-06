import sys
import datetime

# Sample program to be initiated by the Simio Step RunPython
# which can run python scripts with arguments using a specified Python interpreter.

def logit( message ):
    dt = datetime.datetime.now()
    print(dt.strftime("[%H:%M:%S.%f] "), message)  

# redirect stdout to a file

from contextlib import redirect_stdout

try:

    with open('c:\\test\\PythonScriptTakingArgumentsOutput.txt', 'a') as f:
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
