import zipfile
import os
import datetime
import re

###########################################################################################
# Global / Utility
###########################################################################################

printPrefix = "[build_zip_plugin.py]: "

warn = "[WARNING] " #"⚠️  "
err = "[ERROR] " #"⛔ "
info = "[INFO] " #"📎 "

def pprint(any):
    print(f"{datetime.datetime.now()} {printPrefix}{any}")


def stop(success):
    if(success):
        #pprint("✔ Completed successfully")
        pprint("Completed successfully")
    else:
        #pprint("✘ Ended prematurely")
        pprint("Ended prematurely")
        exit()

###########################################################################################
# zip files
###########################################################################################

regInclude = ["(?<!\.orig)\.js$", "(?<!\.orig)\.css$", "(?<!\.orig)\.png$", "(?<!\.orig)\.svg$", "(?<!\.orig)\.php$" ]
fileExclude = ["additional.css"]

def zip_files(directory):
    with zipfile.ZipFile('MusicDatabaseGenerator.zip', 'w') as zipf:
        for root, _, files in os.walk(directory):
            for file in files:
                if fileExclude.__contains__(file):
                    continue
                for reg in regInclude:
                    if re.search(reg, file) is not None:
                        pprint(file)
                        zipf.write(os.path.join(root, file), os.path.relpath(os.path.join(root, file), directory))

zip_files('./')