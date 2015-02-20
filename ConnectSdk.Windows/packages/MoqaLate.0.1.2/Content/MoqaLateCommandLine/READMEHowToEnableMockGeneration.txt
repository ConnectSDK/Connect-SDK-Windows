
To enable generation of mocks add the following to this projects Post-build event command line:

$(ProjectDir)\MoqaLateCommandLine\MoqaLate.exe "$(SolutionDir)$(ProjectName)" "$(SolutionDir)[test project directory]\MoqaLateGeneratedMocks"

Now do a build and include the MoqaLateGeneratedMocks in your test project - mocks!

For example say you had the following directory structure:

MyAwesomeWin7Solution
-MyAwesomeWin7Project
-MyAwesomeWin7TestProject

In the post-build for MyAwesomeWin7Project you would add:

$(ProjectDir)\MoqaLateCommandLine\MoqaLate.exe "$(SolutionDir)$(ProjectName)" "$(SolutionDir)MyAwesomeWin7TestProject\MoqaLateGeneratedMocks"

After you do a build you will get:

MyAwesomeWin7Solution
-MyAwesomeWin7Project
-MyAwesomeWin7TestProject
 -MoqaLateGeneratedMocks

In Visual Studio you would now just include the MoqaLateGeneratedMocks directory in MyAwesomeWin7TestProject project.