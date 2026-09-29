# 当前任务

任务编号：

P-001

任务名称：

序章精修第一轮

场景：

Prologue

状态：

Ready for Codex

---

# 本轮目标

修复当前序章中最明显的视觉和交互问题。

不要进入第一章。

---

# 当前问题

1. 小禾现有图片自带背景，直接显示比较突兀。
2. 当前研学手册只是矩形 + 文字。
3. 手册尺寸偏小。
4. 缺少鼠标跟随动态特效。
5. 游戏还没有完整的音频结构。
6. 当前对话人物显示逻辑需要改变。
7. 志愿者正式图片暂缺。
8. 按钮缺少统一美术结构和 Hover / Pressed 动效。
9. 除标题外，所有文字需要统一使用汉仪傲娇体简。

---

# A. 对话人物显示系统

建立或整理：

PortraitDisplayController

规则：

小禾说话：

只显示 XiaoHe。

志愿者说话：

只显示 Volunteer。

旁白、系统、任务提示：

两者全部隐藏。

不要再保留：

“两个角色同时出现，非说话角色变暗”

这种旧规则。

切换人物时：

0.15 ～ 0.3 秒淡入淡出。

志愿者当前可以用占位 Image。

---

# B. 小禾图片处理

不要强行自动抠图。

建立：

PortraitFrame

要求：

- 圆角或柔和卡片式容器
- 浅边框
- 浅阴影
- 可使用 Mask
- 小禾图片放入容器内部

目的：

弱化当前图片自带背景带来的突兀感。

以后替换透明 PNG 时，不需要改逻辑。

---

# C. 研学手册重做

建立或整理：

HandbookCoverRoot

HandbookCoverImage

HandbookGlow

HandbookHintText

HandbookContentPanel

---

## 手册封面

HandbookCoverImage：

必须是 Inspector 可替换 Sprite。

尺寸：

明显比现在更大。

位置：

画面视觉中心附近。

Hover：

Scale 1.00 → 1.05 左右。

同时轻微高亮。

Pressed：

轻微缩小，再回弹。

---

## 手册打开过程

点击后不要瞬间显示内容。

流程：

封面轻微缩小

↓

向前放大

↓

模拟展开

↓

显示 HandbookContentPanel

整体约：

0.3 ～ 0.6 秒。

预留：

Book Open SFX。

---

## 手册内容

显示：

研学手册

记忆收集：0 / 4

四块记忆：

红色记忆 —— 未解锁

乡土记忆 —— 未解锁

青春记忆 —— 未解锁

乡村记忆 —— 未解锁

点击未解锁项：

显示：

这段记忆还没有被找到……

---

# D. 鼠标跟随动态特效

建立：

CursorEffectController

效果：

- 柔和小光点
- 小粒子
- 短拖尾

要求：

- 鼠标移动时明显
- 静止时减弱
- 不阻挡 UI Raycast
- 后续所有场景可复用

---

# E. 按钮统一动画

建立或复用：

UIButtonAnimator

序章需要接入：

- 下一句
- 剧情选择
- 手册
- 开始寻找
- 手册相关按钮

Hover：

- 轻微变亮
- Scale 约 1.03 ～ 1.05

Pressed：

- Scale 约 0.95 ～ 0.98
- 松开回弹

预留：

UI Hover SFX

UI Click SFX

---

# F. 字体

创建：

Assets/Fonts/HYAoJiaoTiJian/

如果字体尚未导入：

不要伪造字体资源。

但请整理非标题 TMP_Text，使以后能统一替换字体。

本轮完成后在报告里注明：

等待用户提供汉仪傲娇体简字体文件。

---

# G. 音频

建立：

Assets/Audio/BGM/Prologue/

Assets/Audio/SFX/UI/

Assets/Audio/SFX/Dialogue/

Assets/Audio/SFX/Reward/

准备音频接口：

- Prologue BGM
- UI Hover
- UI Click
- Dialogue Advance
- Book Open
- Quest Popup
- Memory Reward

本轮不下载音乐。

---

# H. 正式美术资源

本轮不要自行生成最终美术。

等待 ChatGPT 后续提供：

P_ART_001_Prologue_Background

P_ART_002_Handbook_Cover

P_ART_003_Handbook_Inside

P_ART_004_UI_Button_Set

P_ART_005_Interaction_Icon_Set

---

# 禁止事项

不要：

- 进入 RedMemory 开发
- 改第一章小游戏
- 修改主线剧情
- 从网络自行下载正式背景
- 自行生成志愿者最终形象
- 删除已有可用剧情内容

---

# 验收条件

1. Prologue 可以正常 Play。
2. 小禾说话时只显示小禾。
3. 志愿者说话时只显示志愿者。
4. 系统提示时人物隐藏。
5. 手册明显比现在更大。
6. 手册 Hover / Pressed 正常。
7. 手册图片可以后续直接替换。
8. 手册打开有过渡动画。
9. 鼠标动态效果正常。
10. 鼠标特效不阻挡 UI 点击。
11. 序章所有按钮有 Hover / Pressed。
12. 音频目录和接口已经建立。
13. 无本轮导致的编译错误。
14. 无明显 NullReferenceException。
15. 无明显 Missing Reference。

完成后：

更新 AI_CODEX_REPORT.md。
