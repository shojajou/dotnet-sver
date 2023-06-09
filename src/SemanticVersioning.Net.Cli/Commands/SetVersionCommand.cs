﻿using System.CommandLine;

namespace SemanticVersioning.Net.Commands;

public class SetVersionCommand : Command
{
    private readonly ProjectVersionManager _projectVersionManager;
    
    public SetVersionCommand(ProjectVersionManager projectVersionManager) 
        : base("set", "Sets the version of the chosen project(s).")
    {
        _projectVersionManager = projectVersionManager;
        
        Argument<string> versionArgument = AddVersionArgument();
        Option<int> projectListNumOption = AddProjectListNumOption();
        Option<bool> allOption = AddAllOption();
        
        this.SetHandler(Handle, projectListNumOption, versionArgument, allOption);
        AddValidator(allOption, projectListNumOption);
    }

    private void AddValidator(Option<bool> allOption, Option<int> projectListNumOption)
    {
        base.AddValidator(result =>
        {
            try
            {
                if (result.GetValueForOption(allOption) == false)
                {
                    if (result.GetValueForOption(projectListNumOption) != 0)
                        return;

                    result.ErrorMessage = "To manipulate versions, you must pass --project-number (-p) option\r\n" +
                                          "OR --all option to operate on all projects.";
                }
                else if (result.GetValueForOption(allOption) == true
                         && result.GetValueForOption(projectListNumOption) != 0)
                {
                    result.ErrorMessage = "There is an ambiguity for project selection.\r\n" +
                                          "Please pass either --project-number (-p) option OR --all option for all projects.";
                }
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Something went wrong. Please check your entries again.");
                Console.ResetColor();
            }
        });
    }

    private Option<bool> AddAllOption()
    {
        Option<bool> allOption = new(new[] { "--all", "-a" }, "Sets the version of all the projects.");
        allOption.Arity = ArgumentArity.ZeroOrOne;
        
        AddOption(allOption);
        
        return allOption;
    }

    private Option<int> AddProjectListNumOption()
    {
        Option<int> projectListNumOption = new(new[] { "--project-number", "-p" },
            "Specifies the project number targeting for version upgrading.");
        projectListNumOption.Arity = ArgumentArity.ZeroOrOne;
        
        AddOption(projectListNumOption);
        
        return projectListNumOption;
    }

    private Argument<string> AddVersionArgument()
    {
        Argument<string> versionArgument = new("version");
        versionArgument.AddValidator(validate =>
        {
            if (SemanticVersion.TryParse(validate.GetValueForArgument(versionArgument)) == null)
            {
                validate.ErrorMessage = "<version> argument is invalid.";
            }
        });
        
        AddArgument(versionArgument);
        
        return versionArgument;
    }

    private void Handle(int projectNumber, string version, bool all)
    {
        if (all)
            _projectVersionManager.SetAllVersions(version);
        else
            _projectVersionManager.SetVersion(projectNumber - 1, version);
    }
}