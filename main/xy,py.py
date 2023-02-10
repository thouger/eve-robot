import json

def find_path(json_data, value):
    path = []
    def find_path_helper(json_data, value, current_path):
        if isinstance(json_data, dict):
            for key in json_data:
                if json_data[key] == value:
                    path.append(current_path + [key])
                find_path_helper(json_data[key], value, current_path + [key])
        elif isinstance(json_data, list):
            for i, item in enumerate(json_data):
                if item == value:
                    path.append(current_path + [i])
                find_path_helper(item, value, current_path + [i])
    find_path_helper(json_data, value, [])
    return path

x, y = 0, 0
def read(json_data,x,y):
    if 'dictEntriesOfInterest' in json_data:
        if '_displayX' in json_data['dictEntriesOfInterest'] and not isinstance(json_data['dictEntriesOfInterest']['_displayX'],dict):
            x += json_data['dictEntriesOfInterest']['_displayX']
        if '_displayY' in json_data['dictEntriesOfInterest'] and not isinstance(json_data['dictEntriesOfInterest']['_displayY'],dict):
            y += json_data['dictEntriesOfInterest']['_displayY']
        # if '_text' in json_data['dictEntriesOfInterest'] and 'Local' in json_data['dictEntriesOfInterest']['_text']:
        if '_setText' in json_data['dictEntriesOfInterest'] and 'Local' in str(json_data['dictEntriesOfInterest']['_setText']):
            print(x,y)
    if 'children' in json_data and json_data['children']:
        for child in json_data['children']:
            read(child,x,y)

#∂¡»°memory-reading.json
json_data = json.loads(open('memory-reading.json', 'r').read())
# print(find_path(json_data, "Caldari Food Processing Plant Station"))
read(json_data,x,y)