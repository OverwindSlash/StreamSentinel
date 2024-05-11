# 使用注意事项

#### 1 Detector的更换配置说明

在 `settings.json` 中 detector的配置如下：

```json
"Detector": {
  "AssemblyFile": "Detector.YoloV5Onnx.dll",
  "FullQualifiedClassName": "Detector.YoloV5Onnx.YoloV5OnnxDetector",
  "Parameters": [],
  "ModelPath": "Models\\plate.onnx",
  "UseCuda": true,
  "Thresh": 0.5,
  "ObjType": [ 5, 67 ],
  "YoloVersion": 0
},
```

请暂时不要更换YoloV5OnnxDetector，因为不保证其它的detector实现了同样的功能。

YoloVersion 为 1 时，表示使用自定义的Yolo模型。同时更改模型文件的路径及需要检测的类型ObjType。
