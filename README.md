# Kaiousei Window Capture Proxy
复制目标窗口画面，以达到在OBS捕捉透明窗口的目的。
可以用于捕捉网易云音乐，QQ音乐，酷狗音乐的歌词。

## 系统要求
- Windows 10 (1904版本以上)
- .NET Framework 4.7.2

## 功能
- 将指定窗口的画面映射到本程序的窗口中（包含被硬件加速的窗口）。
- 窗口的大小会自动调整为捕捉目标窗口的大小。
- 可以指定本程序的背景颜色。

## TODO
- [ ] 国际化，并提交到OBS Resources中。
- [ ] 支持`WinRT.GraphicsCapture`的捕捉方式。
- [ ] 优化同步窗口大小的方法。
- [ ] 支持通过NDI或spout2的输出方式。

## License
This project is licensed under the MIT license.