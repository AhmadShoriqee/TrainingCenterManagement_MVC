#!/bin/bash

echo "======================================"
echo "Fixing ALL Commit History"
echo "======================================"
echo ""

# Step 1: Verify we're in a git repository
if [ ! -d ".git" ]; then
    echo "ERROR: Not in a git repository!"
    exit 1
fi

# Step 2: Update local Git config
echo "Step 1: Updating Git configuration..."
git config --global user.email "mahmoudhakim074@gmail.com"
git config --global user.name "MahmoudAlHakim"
echo "✓ Git config updated"
echo ""

# Step 3: Reassign ALL commits to MahmoudAlHakim
echo "Step 2: Rewriting ALL commits..."
echo "WARNING: This will reassign ALL commits to MahmoudAlHakim"
echo ""

git filter-branch -f --env-filter '
export GIT_COMMITTER_NAME="MahmoudAlHakim"
export GIT_COMMITTER_EMAIL="mahmoudhakim074@gmail.com"
export GIT_AUTHOR_NAME="MahmoudAlHakim"
export GIT_AUTHOR_EMAIL="mahmoudhakim074@gmail.com"
' -- --all

echo ""
echo "✓ All commits have been rewritten"
echo ""

# Step 4: Verify the changes
echo "Verifying changes..."
echo "Recent commits:"
git log --oneline -10
echo ""

# Step 5: Push the changes
echo "Step 3: Pushing changes to GitHub..."
echo "WARNING: This will force push to the repository!"
echo ""

git push --force --all
git push --force --tags

echo ""
echo "======================================"
echo "SUCCESS! All commits have been fixed!"
echo "======================================"
echo ""
echo "All commits are now assigned to:"
echo "Name: MahmoudAlHakim"
echo "Email: mahmoudhakim074@gmail.com"
echo ""
echo "Next steps:"
echo "1. Team members should run: git pull --rebase"
echo "2. Check the contributors page to verify changes"
echo "3. Refresh the page if needed (GitHub caches results)"
