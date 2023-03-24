#读取json文件
import json

# Open the file
with open("1.json", "r") as f:
    # Load the data from the file
    data = json.load(f)

for i in data['children']:
    if 'Sujarento' in str(i):
        print()