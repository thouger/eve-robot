# #coding=utf-8
# import json
#
# ParsedUserInterface = {
#     'overview':'OverviewScrollEntry',
# }
#
# def find_path(json_data, value):
#     path = []
#     def find_path_helper(json_data, value, current_path):
#         if isinstance(json_data, dict):
#             for key in json_data:
#                 if json_data[key] == value:
#                     path.append(current_path + [key])
#                 find_path_helper(json_data[key], value, current_path + [key])
#         elif isinstance(json_data, list):
#             for i, item in enumerate(json_data):
#                 if item == value:
#                     path.append(current_path + [i])
#                 find_path_helper(item, value, current_path + [i])
#     find_path_helper(json_data, value, [])
#     return path
#
# x, y = 0, 0
# def read(json_data,x,y,parent=None):
#     if 'dictEntriesOfInterest' in json_data:
#         if '_displayX' in json_data['dictEntriesOfInterest'] and not isinstance(json_data['dictEntriesOfInterest']['_displayX'],dict):
#             x += json_data['dictEntriesOfInterest']['_displayX']
#         if '_displayY' in json_data['dictEntriesOfInterest'] and not isinstance(json_data['dictEntriesOfInterest']['_displayY'],dict):
#             y += json_data['dictEntriesOfInterest']['_displayY']
#
#
#         if '_text' in json_data['dictEntriesOfInterest']:
#             # print(str(json_data['dictEntriesOfInterest']['_setText']))
#             if 'Abandoned Mining Colony' in str(json_data['dictEntriesOfInterest']['_text']):
#                 return True
#
#     if 'children' in json_data and json_data['children']:
#         for child in json_data['children']:
#             result = read(child,x,y,json_data['children'][0])
#             if result:
#                 print(x+json_data['dictEntriesOfInterest']['_displayWidth']['int_low32']/2,y+json_data['dictEntriesOfInterest']['_displayHeight']/2)
#                 break
#
# #��ȡmemory-reading.json
# json_data = json.loads(open(r"D:\code\eveonline-robot\main\bin\Debug\net7.0\111111111111111.txt", 'r').read())
# # print(find_path(json_data, "Caldari Food Processing Plant Station"))
# read(json_data,x,y)
import time

import pyautogui as pag
while True:
    time.sleep(0.5)
    x, y = pag.position()
    print(x, y)