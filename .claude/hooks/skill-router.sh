#!/bin/bash
# Reads prompt from stdin JSON, routes to the most relevant skill(s).
# Fallback: always triggers at least 1 skill.

P=$(jq -r '.prompt // ""')
LOWER=$(echo "$P" | tr '[:upper:]' '[:lower:]')
SKILLS=()
DOCS=""

BE="/Users/hai.phan/Desktop/haiphan/be-window-lamour/src/Lamour.Application/Features"
APP="/Users/hai.phan/Desktop/haiphan/desktop-lamour/src/DesktopLamour/Features/HomePage"

# ── Skill matchers (ordered: specific → general) ─────────────────────────────

# ct-bugfix-skill — runtime errors, crashes, exceptions
echo "$LOWER" | grep -qiE "error|crash|fail|exception|fatal|bug|fix|broken|500|lỗi|không chạy|deadlock|jwt.*issue|di.*error|build.*error|does not conform" \
  && SKILLS+=("ct-bugfix-skill")

# ct-alternative-approaches — trade-off analysis, comparing options
echo "$LOWER" | grep -qiE "best way|alternative|options|approach|compare|which.*better|có cách|nên dùng|phương án|trade.?off|pros.*cons|so sánh.*cách" \
  && SKILLS+=("ct-alternative-approaches")

# review-code — code review
echo "$LOWER" | grep -qiE "review|check.*code|is this correct|is this right|audit|kiểm tra.*code|xem lại.*code|đúng không|code.*ổn không" \
  && SKILLS+=("review-code")

# ct-be-to-desktop — full BE + WPF end-to-end workflow
echo "$LOWER" | grep -qiE "wpf|desktop.*wire|full workflow|end.?to.?end|be.*desktop|desktop.*api|toàn bộ quy trình|wire.*desktop" \
  && SKILLS+=("ct-be-to-desktop")

# ct-feature-pipeline — complete new feature from scratch (all 4 layers, sequential)
echo "$LOWER" | grep -qiE "full feature|feature pipeline|feature.*from scratch|toàn bộ feature|feature mới hoàn chỉnh|pipeline.*feature|làm.*chức năng|tạo.*chức năng|implement.*chức năng|chức năng.*mới|làm.*tính năng|tính năng.*mới|thêm.*tính năng" \
  && SKILLS+=("ct-feature-pipeline")

# ct-module — new complete module (Entity + Repo + UseCase + Controller + DI)
echo "$LOWER" | grep -qiE "new module|complete module|thêm module|module mới|new business feature|tạo module|generate.*module|tạo.*màn hình|màn hình.*mới|tạo.*api.*mới" \
  && SKILLS+=("ct-module")

# ct-generate-usecase — add a single UseCase across layers
echo "$LOWER" | grep -qiE "usecase|use.?case|business operation|add.*operation|thêm usecase|tạo usecase|generate.*usecase|thêm.*nghiệp vụ|logic.*nghiệp vụ" \
  && SKILLS+=("ct-generate-usecase")

# ct-repository — Repository interface + EF Core implementation
echo "$LOWER" | grep -qiE "repository|data access|ef.*query|truy vấn dữ liệu|tạo repository|thêm repository|generate.*repo" \
  && SKILLS+=("ct-repository")

# ct-scaffold — single file generation for an existing feature
echo "$LOWER" | grep -qiE "scaffold|single file|add.*file|tạo file|generate.*file|thêm file.*vào|tạo thêm file" \
  && SKILLS+=("ct-scaffold")

# ct-target — add a new Controller action / endpoint
echo "$LOWER" | grep -qiE "new endpoint|add endpoint|controller action|route mới|thêm endpoint|tạo route|thêm action" \
  && SKILLS+=("ct-target")

# ct-handle-usecase — wire UseCase into existing Controller
echo "$LOWER" | grep -qiE "wire.*usecase|inject.*usecase|wiring|kết nối usecase|đăng ký usecase|register.*usecase|hook.*usecase" \
  && SKILLS+=("ct-handle-usecase")

# ct-cell — DTO / data contract generation
echo "$LOWER" | grep -qiE "\bdto\b|request dto|response dto|data model|data contract|tạo dto|thêm dto|generate.*dto" \
  && SKILLS+=("ct-cell")

# ct-unittest — xUnit + Moq test generation
echo "$LOWER" | grep -qiE "unit test|xunit|moq|kiểm thử|viết test|tạo test|generate.*test|test.*usecase|test.*controller" \
  && SKILLS+=("ct-unittest")

# ct-service — typed HttpClient for calling external APIs
echo "$LOWER" | grep -qiE "httpclient|external api|third.?party|microservice|gọi api ngoài|typed.*client|service.*http|http.*service" \
  && SKILLS+=("ct-service")

# ct-figma-implement-design — implement BE from API contract / Swagger spec
echo "$LOWER" | grep -qiE "api contract|swagger spec|client contract|implement.*spec|from.*design|từ.*spec|từ.*contract|match.*contract" \
  && SKILLS+=("ct-figma-implement-design")

# ct-figma-storyboard — Swagger/OpenAPI annotations + HTTP integration tests
echo "$LOWER" | grep -qiE "swagger|openapi|annotation|integration test|http.*test|tạo swagger|viết.*swagger|document.*api" \
  && SKILLS+=("ct-figma-storyboard")

# ct-quality-engineer — QE/QA validation of implementation
echo "$LOWER" | grep -qiE "validate.*impl|quality|qe\b|qa\b|kiểm định|đảm bảo chất lượng|quality engineer|check.*implementation|verify.*impl" \
  && SKILLS+=("ct-quality-engineer")

# ct-git-diff — structured branch comparison
echo "$LOWER" | grep -qiE "git diff|compare.*branch|branch.*compare|so sánh.*branch|changes.*branch|diff.*branch" \
  && SKILLS+=("ct-git-diff")

# ct-semantic-filter — extract BE-relevant content from PRD/requirements
echo "$LOWER" | grep -qiE "prd|business requirement|feature description|lọc yêu cầu|filter.*requirement|semantic.*filter|trích xuất.*yêu cầu" \
  && SKILLS+=("ct-semantic-filter")

# ct-anti-hallucination — verify classes/interfaces/routes exist before generating
echo "$LOWER" | grep -qiE "verify.*exist|check.*exist|hallucination|kiểm tra.*tồn tại|does.*class.*exist|namespace.*correct|check.*namespace" \
  && SKILLS+=("ct-anti-hallucination")

# ct-theme — response formatting, middleware, GlobalExceptionHandler
echo "$LOWER" | grep -qiE "response format|error.*handling|middleware|globalexception|format.*response|error.*envelope|exception.*handler|response.*convention" \
  && SKILLS+=("ct-theme")

# cocoapods-to-spm (NuGet/package management in this project)
echo "$LOWER" | grep -qiE "nuget|package|dependency|thư viện|cài package|add.*package|install.*package|upgrade.*package" \
  && SKILLS+=("cocoapods-to-spm")

# revert-spm-to-cocoapods (EF Core migration rollback in this project)
echo "$LOWER" | grep -qiE "rollback.*migration|revert.*migration|undo.*migration|xóa migration|drop.*migration|migration.*rollback|remove.*migration" \
  && SKILLS+=("revert-spm-to-cocoapods")

# ct-chotot-module-context — architecture navigation, layer structure, DI
echo "$LOWER" | grep -qiE "module.*structure|architecture|folder.*structure|which.*layer|navigate.*code|di.*setup|cấu trúc dự án|layer nào|file.*ở đâu" \
  && SKILLS+=("ct-chotot-module-context")

# swiftui-design-system (BE response contract in this project)
echo "$LOWER" | grep -qiE "response.*contract|json.*format|snake.?case|api.*format|chuẩn.*response|response.*shape|response.*envelope" \
  && SKILLS+=("swiftui-design-system")

# ct-ai-document — feature documentation writing
echo "$LOWER" | grep -qiE "\bdocument\b|documentation|ghi lại|viết tài liệu|tạo doc|write.*doc|feature.*doc|tài liệu.*feature" \
  && SKILLS+=("ct-ai-document")

# ct-ai-persona-pattern — structured requirements gathering before implementation
echo "$LOWER" | grep -qiE "ai persona|gather.*requirements|ask.*requirements|hỏi.*yêu cầu|trước khi implement|before.*implement|requirements.*first" \
  && SKILLS+=("ct-ai-persona-pattern")

# ct-flipped-interaction — vague/incomplete requests needing clarification
echo "$LOWER" | grep -qiE "unclear|vague|yêu cầu chưa rõ|chưa biết|cần làm gì|không chắc|chưa có spec|need.*clarif" \
  && SKILLS+=("ct-flipped-interaction")

# ct-chain-of-thought — complex design/analysis requiring step-by-step reasoning
echo "$LOWER" | grep -qiE "design|complex|analyze|tư duy|phân tích|thiết kế|how.*approach|step.*step|deep.*dive|brainstorm|think.*through" \
  && SKILLS+=("ct-chain-of-thought")

# ── Feature doc matchers ────────────────────────────────────────────────────
check_docs() {
  local pattern="$1" be_feat="$2" app_feat="$3"
  if echo "$LOWER" | grep -qiE "$pattern"; then
    [ -n "$be_feat"  ] && [ -d "$BE/$be_feat"   ] && DOCS="${DOCS}  BE:  $BE/$be_feat/\n"
    [ -n "$app_feat" ] && [ -d "$APP/$app_feat"  ] && DOCS="${DOCS}  App: $APP/$app_feat/\n"
  fi
}

check_docs "employee|nhan vien|nhân viên"           "Employees/docs"  "Employees/docs"
check_docs "customer|khach hang|khách hàng"          "Customers/docs"  "Customers/docs"
check_docs "product|san pham|sản phẩm"               "Products/docs"   "ProductList/docs"
check_docs "supplier|nha cung cap|nhà cung cấp"      "Suppliers/docs"  "Suppliers/docs"
check_docs "auth|login|dang nhap|đăng nhập"          "Auth"            ""
check_docs "home|tong quan|tổng quan"                ""                "Home/docs"

# ── Fallback: always trigger at least 1 skill ───────────────────────────────
[ ${#SKILLS[@]} -eq 0 ] && SKILLS+=("ct-chain-of-thought")

# ── Output block ─────────────────────────────────────────────────────────────
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

echo "🔧 Skills auto-triggered:"
for s in "${SKILLS[@]}"; do
  echo "   ▶ /$s"
done

if [ -n "$DOCS" ]; then
  echo ""
  echo "📚 Read docs before coding:"
  printf "%b" "$DOCS"
fi

echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

exit 0
