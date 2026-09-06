#!/bin/bash

# Script to fix all commits with wrong email addresses
# This script will rewrite the commit history to use the correct email

echo "======================================"
echo "Fixing Commit History"
echo "======================================"
echo ""

# Step 1: Update local Git config
echo "Step 1: Updating Git configuration..."
git config --global user.email "mahmoudhakim074@gmail.com"
git config --global user.name "MahmoudAlHakim"
echo "✓ Git config updated"
echo ""

# Step 2: Fix all commits with wrong emails
echo "Step 2: Rewriting commit history..."
echo "This may take a while..."
echo ""

git filter-branch --env-filter '
if [ "$GIT_COMMITTER_EMAIL" = "@#1234mahk" ] || [ "$GIT_AUTHOR_EMAIL" = "@#1234mahk" ]
then
    export GIT_COMMITTER_EMAIL="mahmoudhakim074@gmail.com"
    export GIT_COMMITTER_NAME="MahmoudAlHakim"
    export GIT_AUTHOR_EMAIL="mahmoudhakim074@gmail.com"
    export GIT_AUTHOR_NAME="MahmoudAlHakim"
fi

if [ "$GIT_COMMITTER_EMAIL" = "Ahmad Shoriqee@DESKTOP-RPDSES4" ]
then
    export GIT_COMMITTER_EMAIL="mahmoudhakim074@gmail.com"
    export GIT_COMMITTER_NAME="MahmoudAlHakim"
    export GIT_AUTHOR_EMAIL="mahmoudhakim074@gmail.com"
    export GIT_AUTHOR_NAME="MahmoudAlHakim"
fi
' -- --all

echo ""
echo "✓ Commit history rewritten"
echo ""

# Step 3: Push the changes
echo "Step 3: Pushing changes to GitHub..."
echo "WARNING: This will force push to the repository!"
echo "Make sure all team members have pulled the latest changes before running force push"
echo ""

read -p "Do you want to continue with force push? (yes/no): " confirm

if [ "$confirm" = "yes" ] || [ "$confirm" = "y" ]; then
    git push --force --all
    git push --force --tags
    echo ""
    echo "✓ Changes pushed successfully!"
    echo ""
    echo "======================================"
    echo "All commits have been fixed!"
    echo "======================================"
    echo ""
    echo "Next steps:"
    echo "1. Team members should run: git pull --rebase"
    echo "2. Check the contributors page to verify changes"
else
    echo "Force push cancelled. To push later, run:"
    echo "  git push --force --all"
    echo "  git push --force --tags"
fi
