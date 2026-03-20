# 歌词系统与烟雾扰动效果部署指南

## 文件与目录
- Assets/Resources/song.lrc：测试歌词文件（可替换）
- Assets/Scripts/Lyrics/LyricsManager.cs：滚动歌词（解析/高亮/居中/淡入淡出）
- Assets/Editor/LrcImporter.cs：.lrc 导入为 TextAsset，支持拖拽赋值
- Assets/Scripts/MusicPlayToggleButton.cs：播放/暂停一体按钮逻辑（从暂停处继续）
- Assets/Shaders/UI/SmokeDistort.shader：UI 图像烟雾扰动

## 场景配置
- 创建 Canvas → Scroll View
  - Viewport：添加 RectMask2D
  - Content：顶对齐，建议挂 VerticalLayoutGroup（Child Alignment=Upper Center，Force Expand Height 关、Width 开）
  - Viewport 高度=三行显示高度
    - 无布局：3×行高
    - 有布局：3×行高 + 2×Spacing + PaddingTop + PaddingBottom
- 行预制体 LinePrefab（TextMeshProUGUI）
  - RectTransform 高度=行高（例如 36）
  - Anchors：Min=(0,0.5) Max=(1,0.5) Pivot=(0.5,0.5)
  - TextMeshProUGUI：Alignment=Center，Overflow=Overflow
- LyricsManager
  - AudioSource：拖入播放音频的 AudioSource
  - ScrollRect/Content：指向 Scroll View
  - LinePrefab：拖入上面的行预制体
  - Lrc Asset：直接拖入 .lrc（推荐）；或使用 Lrc File Name+Resources 加载
  - SmoothTime/HighlightScale/FadeInTime/FadeOutTime：按需调整

## 播放控制按钮
- 在按钮上挂 MusicPlayToggleButton
  - MusicPlayer：拖入 MusicPlay 组件
  - AudioSourceRef：拖入同一个 AudioSource
  - Label：按钮内 TMP_Text（可选）
  - OnPlayUI/OnPauseUI：挂 UI 动画或图标切换（非必须）
- 不要在按钮的 OnClick 再绑定播放/暂停方法，脚本内部已有切换与去重。

## 歌词文件
- 推荐直接拖拽到 LyricsManager.lrcAsset
- 或放到 Assets/Resources，设置 Lrc File Name（不带扩展名）
- 支持 [mm:ss] 与 [mm:ss.xxx]，支持一行多个时间戳

## 烟雾扰动效果（UI）
- 创建材质，使用 UI/SmokeDistort 着色器
- 将材质赋给 Image 的 Material
- 参数
  - NoiseTex：平铺噪声纹理
  - Intensity：扰动强度（建议 0.005–0.02）
  - Speed：扰动速度与方向
  - Tiling：噪声平铺

## 打包与运行
- TextMeshPro 字体需包含中文字形或使用动态字体资产
- 若使用 Resources 加载，确保 lrc 在对应路径；拖拽赋值无需 Resources
- Editor/LrcImporter 在编辑器中生效，无需运行时依赖

## 常见问题
- 看不到歌词：检查 Lrc Asset 或 Lrc File Name，确认 Content/LinePrefab 引用正确
- 行高不一致：保证 LinePrefab 高度固定，或为其加 LayoutElement.Preferred Height
- 只显示三行：调整 Viewport 高度并使用 RectMask2D 裁剪
