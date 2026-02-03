====================================================================
2D纸片人角色控制系统 - 功能总结
====================================================================

【已实现的功能】

1. ✅ 2D纸片人左右翻转
   - 通过 transform.localScale.x 的正负值控制镜像翻转
   - 按A键 -> 翻转向左（Scale.x为负）
   - 按D键 -> 翻转向右（Scale.x为正）
   - 始终面对摄像机

2. ✅ 前后两组动画切换
   - FacingAway参数控制动画组：
     * FacingAway = false -> 使用Front动画组（面对摄像机）
     * FacingAway = true -> 使用Back动画组（背对摄像机）
   
   - 按W键：
     * 角色向前移动
     * 切换到Back动画组（Walk_Back / Run_Back）
     * _isFacingAway = true
   
   - 按S键：
     * 角色向后移动
     * 切换到Front动画组（Walk_Front / Run_Front）
     * _isFacingAway = false
   
   - 只按A/D键：
     * 保持Front动画组（面对摄像机）
     * 通过Scale翻转实现左右转向

3. ✅ Idle方向记忆
   - 记录最后一次按的前后键（W或S）
   - 松开按键进入Idle时：
     * 如果最后按的是W -> Idle_Back（背对）
     * 如果最后按的是S -> Idle_Front（面对）
     * 如果只按过A/D -> Idle_Front（默认面对）

4. ✅ 修复了Shift加速问题
   - 必须按住Shift才能加速
   - 松开Shift立即恢复为Walk

5. ✅ 修复了动画卡顿问题
   - 使用Bool和Float参数替代Trigger
   - 避免多个动画同时触发

【动画参数】

代码中使用的Animator参数：
1. Speed (Float): 0=Idle, 1=Walk, 2=Run
2. Grounded (Bool): true=在地面, false=在空中
3. Jump (Trigger): 起跳瞬间触发一次
4. FacingAway (Bool): false=面对摄像机, true=背对摄像机

【需要的动画素材】

面对摄像机组（Front）：
- Idle_Front
- Walk_Front
- Run_Front
- Jump_Front

背对摄像机组（Back）：
- Idle_Back
- Walk_Back
- Run_Back
- Jump_Back

【控制逻辑】

输入组合 -> 动画状态：
- 无输入 -> Idle（根据记忆决定Front/Back）
- W -> Walk_Back / Run_Back（背对）
- S -> Walk_Front / Run_Front（面对）
- A -> Walk_Front / Run_Front（面对 + 翻转向左）
- D -> Walk_Front / Run_Front（面对 + 翻转向右）
- W+A -> Walk_Back / Run_Back（背对 + 翻转向左）
- W+D -> Walk_Back / Run_Back（背对 + 翻转向右）
- S+A -> Walk_Front / Run_Front（面对 + 翻转向左）
- S+D -> Walk_Front / Run_Front（面对 + 翻转向右）
- Shift+移动 -> Run（对应方向）
- Space -> Jump（对应方向）

【下一步操作】

1. 在Unity中打开Animator窗口
2. 按照 Animator2D_Setup_Guide.txt 配置动画状态机
3. 添加动画参数：Speed, Grounded, Jump, FacingAway
4. 设置Blend Tree或独立状态
5. 导入你的8个动画素材
6. 测试运行

【调试提示】

如果翻转方向反了：
在ThirdPersonController.cs的Move函数中找到这行：
localScale.x = _isFacingRight ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);

改为：
localScale.x = _isFacingRight ? -Mathf.Abs(localScale.x) : Mathf.Abs(localScale.x);

====================================================================

