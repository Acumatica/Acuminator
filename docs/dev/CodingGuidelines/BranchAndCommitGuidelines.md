# Git Branch Naming Guidelines
When creating a new branch for a feature or bug fix, please follow the guidelines below to name your branch.
1. The branch name should be descriptive and concise. In this case it is better to add extra details to the name of the branch if needed.
2. The branch name should start with the type of the branch, which can be either "**feature**" or "**bugfix**". This will help to quickly identify the purpose of the branch.
   For example, branch with a new diagnostic is a "feature" branch. Branch with a fix for an existing diagnostic is a "bugfix" branch.
3. Optionally, the branch should include the base branch name, which is usually the "dev" branch. This will help to quickly identify the base branch for the changes.

Some of the Acuminator bugs and features are tracked in Acumatica internal bug tracking system and have a number assigned there. These numbers have format `ATR-XXX`. If that is the case and this number is known, 
then the Git branch related to the bug or feature should include this number in its name. The recommended naming convention for the branch in this case is:
```
{type}/ATR-XXX-{baseBranch}-{shortDescription}
```
For example, for ATR-123 bug fix based on the "dev" branch, the branch name can be:
```
bugfix/ATR-123-dev-fix-null-reference
```

# Commit Guidelines
Deciding on what to include into your commits is a subjective process and it is difficult to provide a universal approach. 
The following rules are provided as a recommendation for developers to keep the Git history of the project clearer and easier to investigate. 

1. Please try to keep the related changes together in a single commit. This is the most subjective rule here, so use your best judgment to decide what changes are related. 
2. Please keep commits granular. If you have a large change that can be logically split into smaller parts, please break it into multiple commits. Do not commit it as a single commit.
   This will make the investigation of the history with Git blame easier.
3. If you have significant formatting-only changes (for example, fixing indentation in the file), please do not mix them with functional changes. It is better to have a separate commit for the formatting changes
   and mention that the commit contains formatting changes in the commit message.
4. The commit message should follow the guidelines described in the next section.

## Commit Message General Guidelines

You should write clear descriptive commit messages that explain the purpose of the commit and the changes made. In general, a good commit message should explain the purpose of the commit and the changes made. 
It should be clear and concise, so that other developers can understand the context of the changes without having to look at the code.

As a general rule, the commit message should not be shorter than 16 characters.

Some of the Acuminator bugs and features are tracked in Acumatica internal bug tracking system and have a number assigned there. These numbers have format `ATR-XXX`. If that is the case and this number is known, 
then the commit message should start with the number of the bug or feature. The recommended format for the commit message in this case is:
```
ATR-XXX: Short description of the changes
```
This will help to quickly identify the purpose of the commit and link it to the corresponding bug or feature in the internal tracking system. 
It also helps to find all commits related to a specific bug or feature by searching for the corresponding number in the commit history.