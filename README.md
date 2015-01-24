# Unity Binary Backup #
This tool is being developed to provide an easy to use tool that keeps unwanted files from your Unity projects out of your git repositories.
The tool scans your root gitignore file to determine which files are excluded from version control and extracts them.

## How it will work ##
After validating that the current working directory is a valid Unity project directory, it scans the root gitignore file of the project.
If it finds a comment that contains "#!UBB!#" (without the ""), the following gitignore entries will be backed up including their .meta files.
The files and metafiles will be copied into another directory, preserving their filestructure, where they can be compressed.

## What this is and isn't ##
This tool is **NO(!)** version control system! It's merely a tool that copies files you don't want under version control into a separate folder. From there you could back them up yourself, compress, modify, eat them or whatever.
Those files could - for example - be uncompressed audio assets. You don't want to have such files to be in your git repositories.
