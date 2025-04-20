# Minecraft Parkour Auto Splitter for LiveSplit

This is an auto splitter component for LiveSplit that automatically splits based on Y-coordinates for Minecraft Java Edition parkour maps.

## Features

- Automatically splits when reaching configured Y-coordinate heights
- Configurable split heights for each checkpoint
- Works with any parkour map
- Easy-to-use settings interface

## Installation

1. Download the latest release from the releases page
2. Close LiveSplit if it's running
3. Extract the downloaded file into your LiveSplit's Components folder
4. Start LiveSplit
5. Right-click LiveSplit and select "Edit Layout"
6. Click the "+" button and go to Control -> Minecraft Parkour Auto Splitter
7. Click "OK" to add it to your layout
8. Click "OK" to close the Layout Editor

## Configuration

1. Right-click LiveSplit and select "Edit Layout"
2. Select the "Minecraft Parkour Auto Splitter" component
3. Click "Settings" to open the configuration panel
4. For each split you want to create:
   - Enter the Y-coordinate height that triggers the split
   - Click "Add Split Height" to add more splits
   - Use "Remove Last Split" to remove unwanted splits
5. The splits will trigger when you reach or exceed each configured height

## Requirements

- LiveSplit 1.8.0 or later
- Minecraft Java Edition
- Windows operating system

## Development

This auto splitter is written in C# and requires the following to build:

- Visual Studio 2019 or later
- .NET Framework 4.8 SDK
- LiveSplit's ComponentUtil

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Known Issues

- Memory reading patterns may need to be updated for different Minecraft versions
- Currently only supports the latest version of Minecraft Java Edition

## License

This project is licensed under the MIT License - see the LICENSE file for details. 