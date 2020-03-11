# NullabilityStats
Commandline tool that gives an estimate per-folder how many lines of code hasn't been nullability annotated

Assumes the entire project has Nullability turned on, and counts files where nullability support has been turned off.
Does a best guess estimate on lines of code (ignoring usings, namespace, `{}`, empty lines and comments), and gives stats per-folder to be able to measure progress on adding nullability annotations.