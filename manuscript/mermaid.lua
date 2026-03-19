-- mermaid.lua — Pandoc Lua filter that renders ```mermaid code blocks to SVG
-- using mmdc (mermaid-cli) with Playwright's ARM-native Chrome.
--
-- Usage: pandoc ... --lua-filter=book/mermaid.lua
--
-- Requires:
--   npm install -g @mermaid-js/mermaid-cli
--   python3 -m playwright install chromium
--
-- The puppeteer config tells mmdc to use Playwright's Chrome binary
-- instead of its own (which may be the wrong architecture).

local puppeteer_config = nil
local img_counter = 0
local img_dir = "manuscript/mermaid-images"

-- Find Playwright's Chrome binary
local function find_chrome()
    local home = os.getenv("HOME") or "/home/vscode"
    local cache = home .. "/.cache/ms-playwright"
    -- Find the chromium directory
    local handle = io.popen("find " .. cache .. " -maxdepth 3 -name chrome -type f 2>/dev/null | head -1")
    if handle then
        local path = handle:read("*l")
        handle:close()
        return path
    end
    return nil
end

-- Create puppeteer config file pointing to Playwright's Chrome
local function ensure_puppeteer_config()
    if puppeteer_config then return puppeteer_config end

    local chrome = find_chrome()
    if not chrome then
        io.stderr:write("mermaid.lua: WARNING — Playwright Chrome not found, mmdc may fail\n")
        puppeteer_config = ""
        return ""
    end

    local tmp = os.tmpname() .. ".json"
    local f = io.open(tmp, "w")
    f:write('{"executablePath": "' .. chrome .. '", "args": ["--no-sandbox", "--disable-gpu", "--disable-dev-shm-usage"]}')
    f:close()
    puppeteer_config = tmp
    io.stderr:write("mermaid.lua: using Chrome at " .. chrome .. "\n")
    return tmp
end

-- Ensure output directory exists
local function ensure_dir(dir)
    os.execute("mkdir -p " .. dir)
end

function CodeBlock(block)
    if block.classes[1] ~= "mermaid" then
        return nil
    end

    img_counter = img_counter + 1
    ensure_dir(img_dir)
    local config = ensure_puppeteer_config()

    -- Use content hash for stable filenames across partial builds
    local hash = pandoc.utils.sha1(block.text)
    local short_hash = hash:sub(1, 8)

    -- Write mermaid source to temp file
    local src_file = os.tmpname() .. ".mmd"
    local f = io.open(src_file, "w")
    f:write(block.text)
    f:close()

    -- Output as PNG (universally supported by LaTeX, no rsvg-convert needed)
    -- Use -w 1600 for high-resolution renders that scale well on portrait pages
    -- -s 3 gives 3x pixel density for crisp text after downscaling
    local out_file = img_dir .. "/diagram-" .. short_hash .. ".png"

    -- Skip rendering if a cached image with this hash already exists
    local cached = io.open(out_file, "r")
    if cached then
        cached:close()
        os.remove(src_file)
        io.stderr:write("  ✓ " .. out_file .. " (cached)\n")
        return pandoc.Para({pandoc.Image({}, out_file)})
    end

    local cmd = "mmdc -i " .. src_file .. " -o " .. out_file .. " -b white -s 3 -w 1600"
    if config and config ~= "" then
        cmd = cmd .. " -p " .. config
    end
    cmd = cmd .. " 2>&1"

    local handle = io.popen(cmd)
    local result = handle:read("*a")
    local ok = handle:close()

    os.remove(src_file)

    if ok then
        io.stderr:write("  ✓ " .. out_file .. "\n")
        return pandoc.Para({pandoc.Image({}, out_file)})
    else
        -- Check if a pre-rendered image already exists (e.g. copied from arxiv-submission)
        local f = io.open(out_file, "r")
        if f then
            f:close()
            io.stderr:write("  ⟳ " .. out_file .. ": using pre-rendered image\n")
            return pandoc.Para({pandoc.Image({}, out_file)})
        end
        io.stderr:write("  ✗ " .. out_file .. ": " .. result .. "\n")
        return nil  -- Keep original code block on failure
    end
end

-- Strip "Previous:" / "Next:" / "Back to:" nav links (useless in PDF)
function Para(para)
    local text = pandoc.utils.stringify(para)
    if text:match("^Previous:") or text:match("^Next:") or text:match("^Back to:") then
        return {}
    end
    return nil
end
