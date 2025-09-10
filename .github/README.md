# GitHub Repository Rulesets

This directory contains GitHub repository rulesets that can be imported via GitHub's "Rulesets" UI to enforce branch protection and repository governance policies.

## Files

### repository-ruleset.json (Comprehensive Protection)

This ruleset provides comprehensive protection for the main branch (`master`) with the following rules:

### Branch Protection Rules
- **Pull Request Requirements**: Requires at least 1 approving review before merging
- **Dismiss Stale Reviews**: Automatically dismisses stale reviews when new commits are pushed
- **Resolve Conversations**: Requires all review conversations to be resolved before merging

### Required Status Checks
- **Format Check**: Ensures code formatting standards are met
- **Build Validation**: Requires successful builds for key platforms:
  - Windows ARM64
  - Windows x64
  - Linux x64
  - macOS x64
- **Up-to-date Policy**: Requires branches to be up-to-date with main before merging

### Push Restrictions
- **Force Push Protection**: Prevents force pushes to the main branch
- **Branch Creation**: Controls branch creation
- **Branch Deletion**: Protects against accidental branch deletion
- **Direct Push Prevention**: Requires all changes to go through pull requests

## How to Import

1. Navigate to your repository on GitHub
2. Go to **Settings** > **Rules** > **Rulesets**
3. Click **New Ruleset** > **Import**
4. Upload the `repository-ruleset.json` file
5. Review the imported settings
6. Click **Create** to apply the ruleset

## Bypass Permissions

The ruleset includes bypass permissions for repository administrators, allowing them to override protections when necessary for maintenance or emergency situations.

## Customization

You can modify the ruleset by:
- Editing the required status check contexts to match your CI/CD pipeline
- Adjusting the number of required approving reviews
- Adding or removing specific protection rules
- Modifying bypass actor permissions

After making changes, re-import the updated ruleset through the GitHub UI.

### basic-ruleset.json (Minimal Protection)

A simpler ruleset with basic branch protection suitable for smaller teams or less complex workflows:

- **Pull Request Requirements**: Requires 1 approving review
- **Basic Status Checks**: Only requires format validation
- **Force Push Protection**: Prevents force pushes to main branches
- **Flexible Policy**: Less strict requirements for easier contribution

## Choosing the Right Ruleset

- **Use `repository-ruleset.json`** for production repositories with rigorous CI/CD requirements
- **Use `basic-ruleset.json`** for development repositories or teams getting started with branch protection

Both rulesets protect both `master` and `main` branches to accommodate different naming conventions.