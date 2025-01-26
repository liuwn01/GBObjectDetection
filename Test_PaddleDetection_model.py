from paddlex import create_model
from PIL import Image, ImageDraw, ImageFont
import os

current_folder = f"{os.path.dirname(os.path.abspath(__file__))}"
test_datas_folder = f"{current_folder}/Datas/Testing"
output_folder = f"{test_datas_folder}/Detected/00.PXOD"
if not os.path.exists(output_folder):
    os.makedirs(output_folder)

def parse_det_result(res, source_f_name):
    newimg = Image.open(source_f_name)
    draw = ImageDraw.Draw(newimg)
    for box in res["boxes"]:
        draw.rectangle(box["coordinate"], outline="blue", width=1)
    from pathlib import Path
    new_file_path = f"{output_folder}/{os.path.splitext(os.path.basename(source_f_name))[0]}.parsed{Path(source_f_name).suffix}"
    newimg.save(new_file_path)

# Traverse all files in a directory, excluding subdirectories
model = create_model(f"{current_folder}/PXODModel")
for item in os.listdir(test_datas_folder):
    item_path = os.path.join(test_datas_folder, item)
    if os.path.isfile(item_path):
        print(item_path)
        output = model.predict(item_path, batch_size=1)
        for res in output:
            res.print(json_format=False)
            res.save_to_img(output_folder)
            res.save_to_json(f"{output_folder}/res_{item}.json")
            parse_det_result(res, item_path)