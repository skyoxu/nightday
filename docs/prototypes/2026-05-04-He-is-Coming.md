# Prototype: He-is-Coming

- Status: active
- Owner: operator
- Date: 2026-05-04
- Related formal task ids: none yet

## Hypothesis
- 加入肉鸽元素，验证复古RPG加肉鸽奖励选择是否好玩。

## Core Player Fantasy
- 验证复古RPG加肉鸽是否有趣，玩家是否会因为随机成长流派而持续投入。

## Minimum Playable Loop
- 地图移动，随机遇敌，进入战斗，我方先行动的自动战斗推进，战后获得一次肉鸽三选一奖励，继续推进到下一场遭遇。

## Game Feature
- 地图移动，随机遇敌，肉鸽奖励三选一，包括新装备、新道具、属性加点、技能或技能升级。

## Core Gameplay Loop
- 每四个小怪为一轮精英怪，第三个精英怪后进入 boss 战；整个 prototype 一共 15 个怪，12 个小怪，2 个精英，1 个 boss。

## Win / Fail Conditions
- 第一个 boss 怪物 HP 为 0 则胜利，角色 HP 为 0 则失败。

## Game Type Specifics
- Game Type: rpg
- Guide Path: docs/game-type-guides/rpg.md
- Character System: HP、攻击力、防御力、暴击率
- World and Exploration: 单张线性地图，0s-10s 按 10% 遇敌概率递增
- Combat System: 我方先行动，自动战斗为主，技能全部为被动技能，战斗结果由属性和被动组合决定

## Prototype Type Kit
- Game Type: rpg
- Kit Path: docs/prototype-type-kits/rpg.md

### Gameplay Flow / GDD Route
- 使用随机遇怪、地图撞怪，还是二者都支持？ 随机遇怪
- 战斗是回合制指令，还是即时碰撞/自动战斗？ 自动战斗，但保持回合推进感，由我方先行动
- 胜利后回到地图，还是进入结算后结束 prototype？ 战胜 boss 后结束 prototype；全流程共 15 个怪，12 个小怪，2 个精英，1 个 boss

### Prototype Scene UI
- 战斗场景需要哪些 UI：HP、指令按钮、战斗日志、技能栏？ 我方角色建模、敌方角色建模、我方属性、敌方属性、我方技能、敌方技能、战斗日志说明
- 地图场景需要哪些 UI：HP、任务提示、小地图、遇怪提示？ 我方建模、随机刷新宝箱（肉鸽三选一奖励）、我方属性、我方技能、敌方技能
- 失败后是直接 Game Over，还是允许 Retry？ 直接 Game Over

## Scope
- In:
  - 单张线性地图移动
  - 随机遇敌进入战斗
  - 自动战斗结算
  - 战后肉鸽三选一奖励
  - 15 怪小流程到 boss 结束
- Out:
  - 长线剧情与任务系统
  - 多角色队伍系统
  - 复杂装备经济
  - 正式任务 refs、acceptance refs、overlay refs

## Success Criteria
- 有一定难度和乐趣
- 不会过难和过于简单
- 肉鸽对战斗流派随机性较为重要

## Promote Signals
- 玩家能完整跑通从地图到 boss 的最小闭环
- 肉鸽奖励会显著改变战斗结果或推进体验
- 自动战斗与属性/被动组合能形成可感知流派差异

## Archive Signals
- 复古RPG加肉鸽方向有趣，但当前自动战斗表现力不足
- 地图、战斗、奖励三段闭环存在信号，但还不足以进入正式任务

## Discard Signals
- 自动战斗无法提供足够乐趣或决策感
- 肉鸽奖励对体验影响过小，流派随机性不成立
- 难度调节始终失衡，短周期内难以修正

## Evidence
- Code paths:
  - Game.Core/Prototypes/HeIsComingPrototypeLoop.cs
  - Game.Godot/Prototypes/He-is-Coming/HeIsComingPrototype.tscn
  - Game.Godot/Prototypes/He-is-Coming/Scripts/HeIsComingPrototype.cs
  - Game.Core.Tests/Prototypes/HeisComingPrototypeLoopTests.cs
  - Tests.Godot/tests/Prototype/HeIsComing/test_he_is_coming_scene.gd
- Logs / media / notes:
  - logs/ci/2026-05-04/prototype-tdd-He-is-Coming-red/

## Decision
- pending

## Next Step
- 跑通 prototype green 与 Godot 侧 smoke 验证，再决定 discard、archive 或 promote。
