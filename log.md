# Change log

## 2026-07-24 — Preserve aliases while cloning V2 test parameters

- Updated `PETEL_V2_Core/ObjectCloning.cs` so `DeepCloneArray` uses one shared, reference-identity clone context for every parameter in the array.
- Added original-to-clone tracking to the Unit4 node, queue, stack, binary-node, array, and reflection clone paths.
- Registered newly created reference objects before recursively cloning their child references, preserving shared references and supporting cycles through node/tree links.
- This fixes tests that pass both a list head and a node inside that list: the teacher now receives a cloned list and the corresponding node within that cloned list.
- Rebuilt `Upload_V2_Net472/tester_payload.tar` from the Release runner and updated core DLL so VPL deployments use this change.
- Verified the Release build of `PETEL_MainTester_V2` and clone regression checks: a cloned node parameter remains the `Next` node in the cloned list, repeated parameter references remain shared, an array-contained queue stays aliased with the same queue passed separately, and all clones are separate from their sources.

## 2026-07-24 — Add `Unit4.NodeInteger`

- Added the requested non-generic `NodeInteger` linked-list node type to `PETEL_V2_Core`.
- Located the `NodeInteger` definition in `PETEL_V2_Core/Unit4.cs` with the other Unit4 structures.
- Moved `NodeInteger` from `Unit4.CollectionsLib` into the `Unit4` namespace; no separate collections namespace is required.
- Rebuilt and repacked `Upload_V2_Net472/tester_payload.tar` after the namespace change so VPL uses `Unit4.NodeInteger`.
- Added `Unit4Helper.BuildNodeIntegerList(int[])`, which constructs a `NodeInteger` chain and returns `null` for a null or empty input array.
- Added explicit cloning support so `NodeInteger` participates in the shared clone graph and preserves a separately passed pointer into the cloned list.
- Added structural comparison and diagnostic snapshot recognition so `NodeInteger` results and parameter state are compared and displayed as linked lists.
- Verified the Release build, the `NodeInteger` list-head-plus-target-pointer clone scenario, structural comparison of the original and clone, the existing end-to-end sample suite (65/65), and the repacked VPL payload.
