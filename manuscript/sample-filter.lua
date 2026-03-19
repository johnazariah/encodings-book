-- sample-filter.lua — Builds a sample PDF with full ToC but only selected chapters.
-- Non-sample chapters appear as gray entries in the ToC with gray headings on
-- otherwise blank pages. Sample chapters render normally with full content.

local sample_prefixes = { "Chapter 1:", "Chapter 5:", "Chapter 19:" }

local function is_sample_chapter(text)
    for _, prefix in ipairs(sample_prefixes) do
        if text:find(prefix, 1, true) then
            return true
        end
    end
    return false
end

function Pandoc(doc)
    local new_blocks = {}
    local in_excluded = false

    for _, block in ipairs(doc.blocks) do
        if block.t == "Header" and block.level == 1 then
            local text = pandoc.utils.stringify(block)

            if is_sample_chapter(text) then
                in_excluded = false
                -- Restore black for ToC entry and chapter heading
                table.insert(new_blocks, pandoc.RawBlock('latex',
                    '\\addtocontents{toc}{\\protect\\color{black}}'))
                table.insert(new_blocks, block)
            else
                in_excluded = true
                -- Gray ToC entry
                table.insert(new_blocks, pandoc.RawBlock('latex',
                    '\\addtocontents{toc}{\\protect\\color{gray}}'))
                -- Gray chapter heading on the page, then reset
                table.insert(new_blocks, pandoc.RawBlock('latex',
                    '{\\color{gray}'))
                table.insert(new_blocks, block)
                table.insert(new_blocks, pandoc.RawBlock('latex', '}'))
            end
        elseif not in_excluded then
            table.insert(new_blocks, block)
        end
        -- All non-Header blocks in excluded chapters are silently dropped
    end

    -- Reset ToC color at the end
    table.insert(new_blocks, pandoc.RawBlock('latex',
        '\\addtocontents{toc}{\\protect\\color{black}}'))

    return pandoc.Pandoc(new_blocks, doc.meta)
end
