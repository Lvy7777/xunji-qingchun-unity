# 当前任务

任务编号：

P-002

任务名称：

第一章「红色记忆」精修第一轮

场景：

RedMemory

状态：

Completed

---

# 本轮目标

在不改变既有剧情主线、不进入第二章精修的前提下，将 RedMemory 从功能原型提升为完整可玩的正式章节：章节标题、开场剧情与轻选择、任务发布、2D 横版探索（5 个碎片、线索钥匙、障碍、生命、安全点与失败重试）、条件终点、历史照片占位交互、结尾剧情、红色记忆奖励、研学手册 1 / 4、第一章完成并可继续到 HometownMemory。

---

# 强制复用与规则

- 复用 `PortraitDisplayController`、`CursorEffectController`、`UIButtonAnimator` 与现有 StudyBook / Handbook 系统，不创建重复系统。
- 小禾说话只显示小禾；志愿者说话只显示志愿者；旁白、系统与标题隐藏两人。
- 章节标题可使用标题字体，其余文字统一预留“汉仪傲娇体简”。
- 正式美术均建立 Inspector 可替换插槽，本轮不自行绘图、不联网下载素材、不编造真实历史资料。
- 剧情与 Gameplay 在同一 RedMemory 场景内切换。
- 完成后实际进入 Play Mode 验证完整流程，检查 Console、空引用、缺失引用与重复触发。

---

# 交付

- 更新 `AI_CODEX_REPORT.md`，记录场景、脚本、Prefab、完整流程、Play Mode / Console 结果、待提供美术与音频、已知问题。
- 提交并推送：`Complete P-002 RedMemory polish first pass`
- 不继续 P-003。
