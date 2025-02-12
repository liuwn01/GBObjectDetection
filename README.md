# GBObjectDetection

###[LabelMe](https://github.com/wkentaro/labelme)
This tool is used for data annotation.
Download from [here](https://objects.githubusercontent.com/github-production-release-asset-2e65be/58374888/9a028538-c566-4b48-a9d9-a66fd43015b8?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=releaseassetproduction%2F20250126%2Fus-east-1%2Fs3%2Faws4_request&X-Amz-Date=20250126T093140Z&X-Amz-Expires=300&X-Amz-Signature=3a4afd2c484b0c951434fcfcbdecb0d1c3c8f25161e7e023c820e32edc2d98cd&X-Amz-SignedHeaders=host&response-content-disposition=attachment%3B%20filename%3DLabelme.exe&response-content-type=application%2Foctet-stream).
```
#cmd
cd /path/to/this/repo
cd Datas
labelme images --labels label.txt --nodata --autosave --output annotations
dir /b


```
###Datas
Datas directory structure description:
```
Datas
- annotations
- images
- Testing
    - *.png or *.jpeg
    - Detected
- label.txt
- Labelme.exe
```
- label.txt record label and it is used for Labelme.exe
- Labelme.exe, data labeling program
- 'Datas/annotations' is used to save the json data labeled by LabelMe
- 'Datas/images' is used to store images that need to be labeled
- 'Datas/Testing': 
    - **The root directory of 'Datas/Testing' stores the images that need to be predicted**
    - **All prediction results are saved in 'Datas/Testing/Detected'**

###Paddle detection

[PaddleX](https://github.com/PaddlePaddle/PaddleX)

- You can download the trained model file from [here](https://ms.portal.azure.com/#view/Microsoft_Azure_Storage/ContainerMenuBlade/~/overview/storageAccountId/%2Fsubscriptions%2F75e2804f-801a-4a5e-9985-c3246b4e1a04%2FresourceGroups%2FTestFeature%2Fproviders%2FMicrosoft.Storage%2FstorageAccounts%2Fweininggen2forcodectlto/path/gb18030/etag/%220x8DCF4B9E927882A%22/defaultEncryptionScope/%24account-encryption-key/denyEncryptionScopeOverride~/false/defaultId//publicAccessVal/None) and unzip the files to PXODModel folder.
    - Model: FasterRCNN-ResNeXt101-vd-FPN
    - The directory structure is as follows:
```
PXODModel
- inference.pdiparams
- inference.pdiparams.info
- inference.pdmodel
- inference.yml
```
        

#####Build the python environment
```
conda remove -n pxod01 --all -y
conda create -n pxod01 python=3.9 -y
conda activate pxod01

#cpu
python -m pip install paddlepaddle==3.0.0b2 -i https://www.paddlepaddle.org.cn/packages/stable/cpu/

pip install paddlex==3.0.0b2

python Test_PaddleDetection_model.py //All prediction results are saved in 'Datas/Testing/Detected/00.PXOD'

```

###Custom Vision

Run the project directly.











