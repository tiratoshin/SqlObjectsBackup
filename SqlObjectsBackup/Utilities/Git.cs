using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using NLog;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;

namespace SqlObjectsBackup
{
    public class Git
    {
        private readonly ILogger logger;

        public Git(ILogger logger)
        {
            this.logger = logger;
        }

        public void Checkout(string branchName, string workingDirectory)
        {
            try
            {
                using (var repo = new Repository(workingDirectory))
                {
                    Branch branch = repo.Branches[branchName];
                    if (branch == null)
                    {
                        logger.Info($"Branch {branchName} does not exist");
                        return;
                    }

                    Commands.Checkout(repo, branch);
                    logger.Info($"Checked out to branch {branchName}");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error checking out branch: {ex.Message}");
            }
        }

        public void Pull(string branchName, string workingDirectory)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "git",
                        Arguments = "pull",
                        WorkingDirectory = workingDirectory,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                Console.WriteLine(output);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error pulling from branch: {ex.Message}, Branch: {branchName}, Working Directory: {workingDirectory}");
                logger.Error($"Error pulling from branch: {ex.Message}");
            }
        }

        public void Push(string repoPath, string branch, List<string> modifiedFiles)
        {
            try
            {
                foreach (var file in modifiedFiles)
                {
                    // output what we're doing
                    Console.WriteLine($"Staging file {file}");
                    logger.Info($"Staging file {file}");

                    var processStage = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "git",
                            Arguments = $"add \"{file}\"",
                            WorkingDirectory = repoPath,
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true,
                        }
                    };

                    processStage.Start();
                    processStage.WaitForExit();
                    if (processStage.ExitCode != 0) 
                    {
                        logger.Error($"Stage command failed with exit code {processStage.ExitCode}. Error output: {processStage.StandardError.ReadToEnd()}");
                    }

                }

                Console.WriteLine($"Committing changes for branch {branch}");
                logger.Info($"Committing changes for branch {branch}");

                var processCommit = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "git",
                        Arguments = $"commit -m \"Committing changes for branch {branch}\"",
                        WorkingDirectory = repoPath,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };

                processCommit.Start();
                processCommit.WaitForExit();

                Console.WriteLine($"Pushing changes to branch {branch}");

                var processPush = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "git",
                        Arguments = $"push origin {branch}",
                        WorkingDirectory = repoPath,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };

                processPush.Start();
                processPush.WaitForExit();

                if (processPush.ExitCode != 0)
                {
                    logger.Error($"Error pushing to branch: {processPush.StandardOutput.ReadToEnd()}");
                }
                else
                {
                    logger.Info($"Pushed changes to branch {branch}");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error pushing to branch: {ex.Message}");
            }
        }

        public (bool, string, string) ExecuteGitCommand(string command, string workingDirectory = null)
        {
            using (var process = new Process())
            {
                process.StartInfo.FileName = "git";
                process.StartInfo.Arguments = command;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                if (!string.IsNullOrEmpty(workingDirectory))
                {
                    process.StartInfo.WorkingDirectory = workingDirectory;
                }

                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                logger.Info(output);
                if (!string.IsNullOrEmpty(error))
                {
                    logger.Error(error);
                }

                process.WaitForExit();

                return (process.ExitCode == 0, output, error);
            }
        }

        public bool CheckoutBranchIfNeeded(string branchName, string workingDirectory)
        {
            var (success, output, error) = ExecuteGitCommand($"checkout {branchName}", workingDirectory);

            if (success)
            {
                if (output.Contains($"Switched to branch '{branchName}'") || output.Contains($"Already on '{branchName}'"))
                {
                    logger.Info($"Successfully checked out the {branchName} branch.");
                    Console.WriteLine($"Successfully checked out the {branchName} branch.");
                    return true;
                }
            }

            logger.Error($"Failed to check out the {branchName} branch: {output}");
            Console.WriteLine($"Failed to check out the {branchName} branch: {output}");
            return false;
        }

        public void PushToGit(string repoPath, string branch, List<string> modifiedFiles)
        {
            try
            {
                var checkedOutSuccessfully = CheckoutBranchIfNeeded(branch, repoPath);

                // If checkout fails, create and checkout the branch
                if (!checkedOutSuccessfully)
                {
                    logger.Info($"Creating branch {branch}");
                    Console.WriteLine($"Creating branch {branch}");
                    ExecuteGitCommand($"checkout -b {branch}", repoPath);
                }

                // Pulling the latest changes
                logger.Info("Pulling Git Repo...");
                var (pullSuccess, _, pullError) = ExecuteGitCommand("pull", repoPath);
                if (pullSuccess)
                {
                    logger.Info("Pulled the latest changes from Git.");
                    Console.WriteLine("Pulled the latest changes from Git.");
                }
                else
                {
                    logger.Error($"Failed to pull the latest changes from Git: {pullError}");
                    Console.WriteLine($"Failed to pull the latest changes from Git: {pullError}");
                    return;
                }

                // Add only modified files to Git
                foreach (var file in modifiedFiles)
                {
                    logger.Info($"Adding changed file {file}...");
                    Console.WriteLine($"Adding changed file {file}...");
                    var (addSuccess, _, addError) = ExecuteGitCommand($"add \"{file}\"", repoPath);
                    if (addSuccess)
                    {
                        logger.Info($"File added to Git: {file}");
                        Console.WriteLine($"File added to Git: {file}");
                    }
                    else
                    {
                        logger.Error($"Failed to add file to Git: {file}. Error: {addError}");
                        Console.WriteLine($"Failed to add file to Git: {file}. Error: {addError}");
                    }
                }

                // Check if there are changes to be committed
                if (modifiedFiles.Count > 0)
                {
                    logger.Info("Committing to git...");
                    var (commitSuccess, _, commitError) = ExecuteGitCommand($"commit -m \"Updated SQL objects on {DateTime.Now}\"", repoPath);
                    if (commitSuccess)
                    {
                        logger.Info($"Committed {modifiedFiles.Count} files to Git.");
                        Console.WriteLine($"Committed {modifiedFiles.Count} files to Git.");
                    }
                    else
                    {
                        logger.Error($"Failed to commit changes to Git: {commitError}");
                        Console.WriteLine($"Failed to commit changes to Git: {commitError}");
                        return;
                    }

                    // Set the upstream and push changes
                    var (pushSuccess, _, pushError) = ExecuteGitCommand($"push -u origin {branch}", repoPath);
                    if (pushSuccess)
                    {
                        logger.Info("Changes pushed to Git successfully.");
                        Console.WriteLine("Changes pushed to Git successfully.");
                    }
                    else
                    {
                        logger.Error($"Failed to push changes to Git: {pushError}");
                        Console.WriteLine($"Failed to push changes to Git: {pushError}");
                    }
                }
                else
                {
                    logger.Info("No changes to commit.");
                    Console.WriteLine("No changes to commit.");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error during Git operations.");
                Console.WriteLine("Error during Git operations.");
            }
        }
    }
}