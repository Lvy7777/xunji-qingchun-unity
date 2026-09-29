# 《寻迹·青春》AI 协作工作流

## 角色分工

### ChatGPT：项目主导

负责：

- 决定开发顺序和当前任务
- 维护剧情与交互设计
- 决定小游戏规则
- 决定 UI 和整体视觉规范
- 生成除尾章外的正式背景图、手册图、UI 图、按钮图和交互图标
- 检查 Codex 的修改结果
- 审查 GitHub 提交
- 决定任务是否通过验收
- 决定下一阶段开发内容

### Codex：Unity 执行方

负责：

- 阅读 AI_MASTER_PLAN.md
- 阅读 AI_CURRENT_TASK.md
- 使用 Unity MCP 修改 Unity 项目
- 编写和修改 C# 代码
- 创建 GameObject
- 创建和调整 UI
- 调整 Inspector
- 接入 ChatGPT 提供的正式图片
- 完成小游戏逻辑
- 检查 Unity Console
- 修复本轮修改产生的问题
- 完成任务后更新 AI_CODEX_REPORT.md
- 将修改提交并 push 到 GitHub

### 用户

主要负责：

- 在本机保持 Unity 与 Codex 可用
- 查看最终运行效果
- 告诉 ChatGPT 哪些地方好看或不好看
- 提供志愿者正式立绘、字体文件和后续找到的音乐文件

---

## 强制规则

1. Codex 不得自行修改主线剧情。
2. Codex 不得自行改变章节顺序。
3. Codex 不得自行更换已经确定的小游戏设计。
4. 除尾章外，正式美术图片由 ChatGPT 生成。
5. Codex 不自行从网络下载正式背景、按钮、手册图或人物图。
6. 所有正式图片必须通过可替换 Image / SpriteRenderer / Inspector 插槽接入。
7. 对话统一规则：
   - 小禾说话：只显示小禾
   - 志愿者说话：只显示志愿者
   - 旁白、系统、标题：两者都隐藏
8. 除章节大标题和主视觉标题外，其余字体全部使用：汉仪傲娇体简。
9. 所有可点击按钮必须有：
   - Normal
   - Hover
   - Pressed
10. Hover：
    - 轻微变亮
    - 轻微放大
11. Pressed：
    - 轻微缩小
    - 松开回弹
12. 全游戏使用统一鼠标跟随动态特效。
13. 每次修改后必须检查：
    - 编译错误
    - NullReferenceException
    - Missing Reference
    - 重复触发
    - 重复点击
    - 场景切换异常
14. 与任务无关的 Package Manager 网络错误不用处理。

---

## Codex 工作前必须阅读

1. AI_WORKFLOW.md
2. AI_MASTER_PLAN.md
3. AI_CURRENT_TASK.md

---

## Codex 工作完成后必须更新

AI_CODEX_REPORT.md

至少包含：

- 完成状态
- 修改场景
- 修改脚本
- 新建 GameObject
- 新建 Prefab
- 测试结果
- Console 情况
- 等待 ChatGPT 提供的图片
- 等待用户提供的资源
- 已知问题
