# Block Puzzle ML-Agents Setup Guide

## Những gì đã được tạo:

### 1. **MoveBlockAgent.cs** - AI Agent chính
- **Observations (Quan sát):**
  - Grid state: 81 giá trị (lưới 9x9) - 1.0f = ô trống, 0.0f = ô đã chiếm
  - Shape data: 27 giá trị (3 shape × 9 giá trị mỗi shape từ ma trận 3x3)

- **Actions (Hành động):**
  - Discrete Action 0: Chọn shape (0, 1, 2)
  - Discrete Action 1: Vị trí đặt trên grid (0-80)

- **Rewards (Phần thưởng):**
  - +1.0f: Đặt shape thành công
  - +0.1f × điểm: Bonus khi hoàn thành hàng/cột/ô 3x3
  - +0.01f: Survival reward mỗi step
  - -0.5f: Hành động không hợp lệ
  - -5.0f: Game over
  - -0.1f: Đạt giới hạn step

### 2. **GameManager.cs** - Quản lý điểm số
- Theo dõi điểm số hiện tại
- Kết nối với GameEvents để cập nhật điểm

### 3. **config.yaml** - Cấu hình training ML-Agents
- PPO algorithm với hyperparameters tối ưu cho puzzle game
- 500,000 steps training

### 4. **Fixes Applied:**
- ✅ Thêm `ResetScore` event vào GameEvents.cs
- ✅ Xóa unused variable `_shapeDraggable` trong Shape.cs
- ✅ Thêm method `GetGridSquares()` trong Grid.cs
- ✅ Thêm method `ClearGrid()` trong Grid.cs cho ML Agent

## Tại sao ML-Agent không hoạt động?

### **Nguyên nhân phổ biến:**
1. **Chưa setup Agent GameObject đúng cách trong Unity**
2. **Chưa cấu hình BehaviorParameters**
3. **Chưa assign references (Grid, ShapeStorage)**
4. **Behavior Name không khớp với config.yaml**
5. **Chưa nhấn Play trong Unity sau khi chạy mlagents-learn**

### **Checklist Setup trong Unity:**

#### **Bước 1: Tạo Agent GameObject**
1. **Tạo Empty GameObject** tên "ML Agent"
2. **Add Component:** `MoveBlockAgent`
3. **Add Component:** `Behavior Parameters` 
4. **Add Component:** `Decision Requester`

#### **Bước 2: Cấu hình Behavior Parameters**
- **Behavior Name:** `MoveBlockAgent` (phải khớp với config.yaml)
- **Behavior Type:** Default
- **Team ID:** 0
- **Use Child Sensors:** ✓ (checked)

**Vector Observation:**
- **Space Size:** 108 (81 grid + 27 shapes)
- **Stacked Vectors:** 1

**Actions:**
- **Continuous Actions:** 0
- **Discrete Branches:** Size = 2
  - **Branch 0 Size:** 3 (number of shapes)
  - **Branch 1 Size:** 81 (grid positions)

#### **Bước 3: Cấu hình Decision Requester**
- **Decision Period:** 5 (agent sẽ quyết định mỗi 5 frame)
- **Take Actions Between Decisions:** ✓ (checked)

#### **Bước 4: Assign References**
- **Grid:** Kéo Grid GameObject vào
- **Shape Storage:** Kéo ShapeStorage GameObject vào

#### **Bước 5: Add Debug Helper**
1. **Add Component:** `MLAgentDebugger` vào cùng GameObject
2. **Assign:** Agent, Grid, ShapeStorage vào debugger
3. **Enable:** Show Debug Info

### **Bước 6: Test trong Unity**
1. **Nhấn Play** trong Unity (không training)
2. **Mở Console** (Window → General → Console)
3. **Kiểm tra debug logs:**
   - `✓ MoveBlockAgent assigned`
   - `✓ Grid assigned` 
   - `✓ ShapeStorage assigned`
   - `✓ BehaviorParameters found`

4. **Test manual action:** Right-click trên MLAgentDebugger → **Test Agent Action**

### **Bước 7: Training với ML-Agents**
```bash
# Trong terminal với mlagents_env activated
mlagents-learn config.yaml --run-id=BlockPuzzle_v1
```

**Sau khi thấy "Start training by pressing the Play button":**
1. **Nhấn Play trong Unity**
2. **Quan sát Console** - phải thấy debug logs:
   - `[ML-Agent] Episode Begin`
   - `[ML-Agent] Step X: Received action`
   - `[ML-Agent] Shape placement...`

### **Troubleshooting:**

**Nếu không thấy debug logs:**
- Kiểm tra Agent GameObject có active không
- Kiểm tra MoveBlockAgent component có enabled không  
- Kiểm tra Console có filter Hide/Collapse không

**Nếu thấy lỗi "Behavior not found":**
- Kiểm tra Behavior Name khớp với config.yaml
- Đảm bảo config.yaml ở đúng thư mục

**Nếu Agent không nhận action:**
- Kiểm tra Decision Requester đã add chưa
- Kiểm tra Decision Period (thử giảm xuống 1-2)

**Nếu placement không hoạt động:**
- Kiểm tra Grid và ShapeStorage có hoạt động bình thường không
- Test game manual trước khi train AI

### Bước 1: Cài đặt ML-Agents
1. Mở **Package Manager** (Window → Package Manager)
2. Chuyển sang **Unity Registry**
3. Tìm và cài đặt **ML Agents**

### Bước 2: Setup Scene
1. **Tạo GameObject mới** cho Agent:
   - Right-click trong Hierarchy → Create Empty
   - Đặt tên: "ML Agent"
   - Attach script **MoveBlockAgent.cs**

2. **Assign References:**
   - Kéo Grid object vào field **Grid** của Agent
   - Kéo ShapeStorage object vào field **Shape Storage** của Agent

3. **Tạo GameManager:**
   - Tạo Empty GameObject tên "GameManager"
   - Attach script **GameManager.cs**

### Bước 3: Cấu hình Agent
1. **Trong MoveBlockAgent component:**
   - **Behavior Name:** "MoveBlockAgent"
   - **Vector Observation Space Size:** 108 (81 grid + 27 shapes)
   - **Actions:**
     - **Discrete Branches Size:** 2
     - **Branch 0 Size:** 3 (number of shapes)
     - **Branch 1 Size:** 81 (grid positions)

### Bước 4: Training
1. **Mở Terminal/Command Prompt**
2. **Navigate** đến thư mục project
3. **Run training command:**
   ```bash
   mlagents-learn config.yaml --run-id=BlockPuzzle_v1
   ```
4. **Nhấn Play** trong Unity khi thấy "Start training by pressing the Play button"

### Bước 5: Testing
- Để test manual control, nhấn **Play** mà không training
- Agent sẽ sử dụng **Heuristic** function (random actions)

## Điều chỉnh nếu cần:

### Nếu training chậm:
- Giảm `max_steps` trong config.yaml
- Tăng `batch_size` và `buffer_size`

### Nếu Agent không học được:
- Điều chỉnh `learning_rate` (thử 1.0e-4)
- Thay đổi reward values trong Agent
- Kiểm tra observation space có đúng không

### Nếu gặp lỗi:
1. **Compile Errors:**
   - Đảm bảo ML-Agents package đã cài đúng
   - Kiểm tra tên **Behavior Name** khớp với config.yaml
   - Đảm bảo Grid và ShapeStorage được assign đúng
   - Các lỗi về GameEvents.ResetScore đã được fix

2. **Runtime Errors:**
   - Kiểm tra GameManager GameObject đã được tạo
   - Đảm bảo Agent component có đầy đủ references
   - Kiểm tra scene có Grid và ShapeStorage hoạt động bình thường

3. **Training Errors:**
   - Đảm bảo config.yaml ở đúng thư mục project
   - Kiểm tra Behavior Name trong Agent khớp với config
   - Đảm bảo Python ML-Agents đã cài đặt: `pip install mlagents`

4. **NullReferenceException trong Grid.ClearGrid():**
   - **Nguyên nhân:** Agent cố gắng clear grid trước khi grid được initialize
   - **Giải pháp:** Đã fix bằng cách thêm null checks và auto-initialization
   - **Kiểm tra:**
     - Đảm bảo Grid GameObject có Grid component
     - Đảm bảo Grid được assign đúng trong MoveBlockAgent
     - Kiểm tra Console có message "Grid not initialized, creating grid first"
   - **Nếu vẫn lỗi:**
     - Thử restart Unity và Play lại
     - Kiểm tra Grid GameObject có active không
     - Đảm bảo GridSquare prefab được assign trong Grid component

5. **NullReferenceException trong GameEvents calls:**
   - **Nguyên nhân:** GameEvents static actions chưa được initialize hoặc GameManager chưa tồn tại
   - **Giải pháp:** Đã fix bằng cách:
     - Thêm null checks với `?.Invoke()` cho tất cả GameEvents calls
     - Tự động tạo GameManager nếu không tồn tại
     - Thêm delay trong OnEpisodeBegin để đảm bảo all components ready
   - **Kiểm tra:**
     - Đảm bảo có GameManager GameObject trong scene
     - Kiểm tra Console có message "GameManager created successfully"
     - Kiểm tra message "Grid cleared successfully" và "New shapes requested successfully"

6. **IndexOutOfRangeException trong Shape.GetCurrentShapeDataSquares():**
   - **Nguyên nhân:** Shape data có kích thước khác với expected 3x3 hoặc chưa được initialize
   - **Giải pháp:** Đã fix bằng cách:
     - Thêm bounds checking trong `GetCurrentShapeDataSquares()` method
     - Đảm bảo luôn return đúng 9 floats (3x3 array)
     - Thêm null checks cho currentShapeData và board arrays
     - Thêm error handling trong CollectObservations method
   - **Kiểm tra:**
     - Đảm bảo ShapeStorage có đủ 3 shapes
     - Kiểm tra Console có message "Shape X active, data length: 9"
     - Kiểm tra không có warning "returned X floats instead of 9"

7. **Agent không sử dụng shapes đúng cách:**
   - **Nguyên nhân:** 
     - ShapeStorage.shapeList chưa được assign đúng 3 Shape objects trong Unity Inspector
     - Agent không hiểu luật game: phải dùng hết 3 shapes mới được shapes mới
     - Không có visual feedback cho AI actions
   - **Giải pháp:** Đã fix bằng cách:
     - Sửa lại game logic đúng với luật: chỉ tạo 3 shapes mới khi đã dùng hết 3 shapes cũ
     - Thêm comprehensive debugging cho shape access
     - Thêm game over detection khi không còn nước đi
     - Thêm visual feedback animation khi Agent đặt shape
     - Thêm bonus reward (+2.0) khi Agent sử dụng hết 3 shapes
   - **Kiểm tra:**
     - Đảm bảo ShapeStorage GameObject trong scene có 3 Shape objects được assign vào shapeList
     - Kiểm tra Console có message "All shapes used! Requesting new batch of 3 shapes..."
     - Kiểm tra message "Bonus reward +2.0 for using all 3 shapes!"
     - Kiểm tra Agent chỉ được shapes mới khi đã dùng hết 3 shapes

8. **Game bị biến dạng khi training ML-Agents:**
   - **Nguyên nhân:** 
     - ML Agent can thiệp vào game mechanics khi training
     - Agent override game flow và làm game không hoạt động bình thường
     - Episode reset quá aggressive làm hỏng game state
   - **Giải pháp:** Đã fix bằng cách:
     - Thêm training mode detection (`Academy.Instance.IsCommunicatorOn`)
     - Chỉ reset game state khi thực sự đang training
     - Tách biệt behavior giữa training mode và normal play
     - Thêm observation-only mode khi không training
     - Thêm MLAgentController để dễ control từ Inspector
   - **Kiểm tra:**
     - Khi chạy `mlagents-learn`: Console hiển thị "Training mode: True"
     - Khi chơi bình thường: Console hiển thị "Training mode: False"
     - Game hoạt động bình thường khi không training
     - Có thể toggle `enableMLAgent` trong Inspector để control

**Cách sử dụng:**
- **Training AI:** Chạy `mlagents-learn config.yaml --run-id=BlockPuzzle_v1` → Game sẽ ở training mode
- **Chơi bình thường:** Không chạy mlagents-learn → Game hoạt động bình thường
- **Control manual:** Set `enableMLAgent = false` trong MoveBlockAgent Inspector

## Game Mechanics hiện tại:
- **Grid:** 9x9 cells
- **Shapes:** 3 shapes được cho một lúc
- **Rule:** Phải sử dụng HẾT 3 shapes thì mới được 3 shapes mới
- **Goal:** Đặt shapes để lấp đầy hàng/cột/ô 3x3
- **Score:** 10 điểm mỗi line completed
- **Game Over:** Khi không thể đặt bất kỳ shape nào trong 3 shapes hiện tại

Agent được thiết kế để học cách:
1. Chọn shape phù hợp trong 3 shapes hiện tại
2. Tìm vị trí tối ưu để đặt
3. Tối đa hóa điểm số và sử dụng hết 3 shapes
4. Tránh game over khi không còn nước đi

**Reward System:**
- **+1.0f:** Đặt shape thành công
- **+2.0f:** Bonus khi sử dụng hết 3 shapes (được shapes mới)
- **+0.1f × điểm:** Bonus khi hoàn thành hàng/cột/ô 3x3
- **+0.01f:** Survival reward mỗi step
- **-0.5f:** Hành động không hợp lệ (chọn shape đã dùng, vị trí không hợp lệ)
- **-5.0f:** Game over
- **-0.1f:** Đạt giới hạn step
