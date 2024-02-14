# BOTW Icon Maker
A tool to automate the sbitemico creation process.
# Download
Get the tool from [here.](https://github.com/CEObrainz/BOTW-Icon-Maker/releases)

Requirement: Requires a minimum of .NET 6.0, which you can download [here.](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)\
Note: Currently, only Wii U sbitemico creation is supported (Switch coming soon).
# Usage
## Command Line
`.\BOTWIconMaker.exe [Version] [Source Folder] [Destination Folder]`

[Version]                Choose either "wiiu" or "switch". If not specified, it defaults to "wiiu". [Optional] \
[Source Folder]          The folder where your images are located. If not specified, it uses the current folder. \
[Destination Folder]     The folder where your files will be saved. If not specified, it creates an "output" folder in the source folder.

#### Examples:
`.\BOTWIconMaker.exe wiiu images output_images`\
`.\BOTWIconMaker.exe switch image_folder`\
`.\BOTWIconMaker.exe wiiu`

## Drag and Drop

You can drag and drop a folder onto the executable, and an output folder will be created within the folder you dropped.

## Run the executable

Simply run the .exe file in the folder you want to use as the source. An output folder will be created with the sbitemico files.

# Limitations

BOTW Icon Maker currently supports the following formats: JPG, JPEG, PNG, GIF and BMP \
Images larger than 512 x 512 will be ignored.
