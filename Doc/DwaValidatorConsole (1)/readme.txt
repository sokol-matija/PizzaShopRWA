------------------------------------------------------------------------------------
DWA Validator - Console Application
------------------------------------------------------------------------------------
Application for validating DWA (Development of Web Applications) project.
Note that you first need to manually package the application after building it in 
Visual Studio, together with the required Database.sql file in its required 
directory. 

Validator will validate the DWA project. 
- If the validation is successful, it will unpack the archive, create database 
  accodrding to content of your Database.sql file and set the connection strings in 
  your project configuration files.
- If the validation is unsuccessful, it will report the errors. Please fix the 
  errors, repack the project and try again.
------------------------------------------------------------------------------------

Usage:
- unpack the archive to your PC somewhere
- open console window and navigate to the folder where you unpacked the archive
- run DwaValidatorConsole.exe command with arguments:
	(a) Windows authentication:
		.\DwaValidatorConsole.exe --input D:\algebra-dwa-validator\dotnet-gui\dwa-validator\examples\ProjectTask-example.zip --output D:\temp\output --db_datasource OzrenXPS\SQLEXPRESS
	(b) SQL Server authentication:
		.\DwaValidatorConsole.exe --input D:\algebra-dwa-validator\dotnet-gui\dwa-validator\examples\ProjectTask-example.zip --output D:\temp\output --db_datasource OzrenXPS\SQLEXPRESS --db_user sa --db_password Pa55w.rd
