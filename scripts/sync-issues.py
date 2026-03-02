import json
import re
import subprocess
import tempfile
import os

# Parse tasks.md
tasks_file = "specs/001-keycloak-sso/tasks.md"
with open(tasks_file, "r") as f:
    content = f.read()

# Build a dictionary of TXXX -> completed (True/False)
task_states = {}
for match in re.finditer(r'- \[([ xX])\] (T\d+)', content):
    state = match.group(1).lower() == 'x'
    task_id = match.group(2)
    task_states[task_id] = state

# Fetch open issues
print("Fetching issues...")
result = subprocess.run(["gh", "issue", "list", "--json", "number,body", "--limit", "100"], capture_output=True, text=True)
if result.returncode != 0:
    print("Error fetching issues:", result.stderr)
    exit(1)

issues = json.loads(result.stdout)

# Process issues
for issue in issues:
    number = issue["number"]
    body = issue["body"]
    
    if not body:
        continue
        
    modified = False
    new_body = body
    
    # Look for tasks in the body
    for match in re.finditer(r'- \[([ \xA0xX])\] (T\d+)', body):
        current_mark = match.group(1).lower()
        task_id = match.group(2)
        
        if task_id in task_states:
            is_completed_in_master = task_states[task_id]
            is_completed_in_issue = current_mark == 'x'
            
            if is_completed_in_master and not is_completed_in_issue:
                # Need to check it in the issue
                print(f"Issue #{number}: Marking {task_id} as done.")
                # We do a simple string replace for this specific task string
                # Be careful to replace exactly this match
                old_str = f"- [{match.group(1)}] {task_id}"
                new_str = f"- [x] {task_id}"
                new_body = new_body.replace(old_str, new_str)
                modified = True
                
    if modified:
        print(f"Updating Issue #{number}...")
        # Write to a temp file
        fd, temp_path = tempfile.mkstemp(suffix=".md")
        with os.fdopen(fd, 'w') as f:
            f.write(new_body)
            
        # Update via gh run
        update_cmd = ["gh", "issue", "edit", str(number), "--body-file", temp_path]
        subprocess.run(update_cmd, check=True)
        os.remove(temp_path)

print("Done syncing issues.")
