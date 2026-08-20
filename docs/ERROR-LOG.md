# Error Log - 3D AI Assistant

## Fixed Errors (Chronological)

### 1. EventSystem Duplicate
- **Cause**: Old scene had duplicate EventSystem objects
- **Fix**: SceneSetup.cs guard + re-run Setup Scene
- **Date**: 2026-08-15

### 2. PlaceholderAvatar Deformation
- **Cause**: VRM not loading, placeholder showing with wrong appearance
- **Fix**: Set VRM path in Inspector + diagnostic logging
- **Date**: 2026-08-15

### 3. Character Facing Backward
- **Cause**: spawnRotation Y=0 in saved scene
- **Fix**: Updated ai.unity to spawnRotation Y=180
- **Date**: 2026-08-15

### 4. T-Pose / Y-Pose
- **Cause**: String-based FindBone failed for VRM bone names (J_Bip_L_UpperArm)
- **Fix**: HumanBodyBones enum via Animator.GetBoneTransform() + rest pose 75deg
- **Date**: 2026-08-16

### 5. Arrow Glitch (VRMLookAtBoneApplyer)
- **Cause**: UniVRM auto-created VRMLookAtHead + VRMLookAtBoneApplyer with null target
- **Fix**: Disable all LookAt components + LockLookAtComponents for 3s
- **Date**: 2026-08-16

### 6. Expression Snap Glitch
- **Cause**: ImmediatelySetValue jumped 0 to 100% in 1 frame
- **Fix**: Smooth Lerp transition (0.3s) + auto-reset after 4s + weight 0.75
- **Date**: 2026-08-17

### 7. Lip Sync Too Large
- **Cause**: Viseme weight too high (amplitude*7)
- **Fix**: Reduced to amplitude*1.5 + single viseme (aa only)
- **Date**: 2026-08-17

### 8. Groq Model 404 Error
- **Cause**: llama-3.1-8b-instant deprecated on Groq
- **Fix**: Changed to qwen/qwen3.6-27b + force load_dotenv(override=True)
- **Date**: 2026-08-18

### 9. Backend Emotion Tag Not Extracted
- **Cause**: Qwen model returns `` blocks that weren't stripped
- **Fix**: Added strip_think_blocks() regex before extract_emotion_tag()
- **Date**: 2026-08-18

### 10. Unity Library Corruption
- **Cause**: PackageCache deleted accidentally during cache cleanup
- **Fix**: Restored from Unity npm cache + recreated .meta files + projectVersion.txt
- **Date**: 2026-08-18

## Last Updated
- 2026-08-18
