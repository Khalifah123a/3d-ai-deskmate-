# TODO List - 3D AI Assistant

## Done
- [x] #1 Chat UI — EventSystem + Legacy UI + proper layout
- [x] #2 VRM Pose — HumanBodyBones + rest pose 75deg + Slerp animation
- [x] #3 Karakter menghadap belakang — spawnRotation Y=180
- [x] #4 Groq model — qwen/qwen3.6-27b (llama-3.1 deprecated)
- [x] #5 Expression glitch — smooth Lerp transition + auto-reset + VRMLookAt disabled
- [x] #6 Arrow glitch — Disabled VRMLookAtBoneApplyer + VRMLookAtHead + SpringBone
- [x] #7 Lip sync too big — Reduced viseme weight (amplitude*1.5, max 1.0)
- [x] #8 Unity Library corruption — Restored PackageCache from npm cache

## Optional Improvements
- [ ] Enable SpringBone physics for hair/clothes animation
- [ ] Enable VRMLookAtHead with proper camera target
- [ ] Add chat history persistence
- [ ] Custom character name/personality
- [ ] Deploy to cloud server

## Last Updated
- Date: 2026-08-18
- All critical bugs fixed. Project functional end-to-end.
