shader_type canvas_item;

uniform vec4 paper_color : source_color = vec4(0.9569, 0.9451, 0.9176, 1.0);
uniform vec3 wash_tint = vec3(0.468, 0.474, 0.442);
uniform float grain_strength = 0.042;
uniform float fiber_strength = 0.028;
uniform float wash_strength = 0.11;
uniform float bloom_strength = 0.08;
uniform float edge_accent_strength = 0.052;
uniform float vignette_strength = 0.038;

float hash(vec2 p)
{
    return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453123);
}

float value_noise(vec2 p)
{
    vec2 cell = floor(p);
    vec2 local = fract(p);
    vec2 blend = local * local * (3.0 - 2.0 * local);

    float a = hash(cell);
    float b = hash(cell + vec2(1.0, 0.0));
    float c = hash(cell + vec2(0.0, 1.0));
    float d = hash(cell + vec2(1.0, 1.0));

    return mix(mix(a, b, blend.x), mix(c, d, blend.x), blend.y);
}

float fbm(vec2 p)
{
    float value = 0.0;
    float amplitude = 0.5;

    // 题屏长期全屏显示时，这个 shader 是主要热点之一，因此把 octave 收口到 3 层。
    for (int octave = 0; octave < 3; octave++)
    {
        value += value_noise(p) * amplitude;
        p = (p * 2.0) + vec2(19.1, 7.3);
        amplitude *= 0.5;
    }

    return value;
}

void fragment()
{
    // 先保留细颗粒与纵向纤维，让底材仍然像宣纸，而不是纯平涂色块。
    vec2 coarse_uv = UV * vec2(240.0, 180.0);
    vec2 fine_uv = UV * vec2(620.0, 460.0);
    float coarse = value_noise(coarse_uv);
    float fine = value_noise(fine_uv);
    float fiber = sin((UV.y * 920.0) + (coarse * 7.2)) * 0.5 + 0.5;
    float paper_noise = ((coarse - 0.5) * grain_strength) + ((fine - 0.5) * grain_strength * 0.48) + ((fiber - 0.5) * fiber_strength);

    // 使用更轻量的低频噪声场生成水墨晕染，兼顾卷轴感与启动页性能。
    vec2 warp = vec2(
        value_noise(UV * vec2(2.0, 1.7) + vec2(1.7, 9.2)),
        value_noise(UV * vec2(1.8, 2.1) + vec2(7.3, 2.8))
    ) - vec2(0.5);
    vec2 wash_uv = (UV * vec2(2.0, 1.7)) + (warp * 0.42);

    float wash_field = fbm(wash_uv);
    float bloom_field = value_noise((wash_uv * vec2(3.2, 2.9)) + vec2(5.4, 1.2));

    float wash_mask = smoothstep(0.48, 0.80, wash_field);
    float bloom_mask = smoothstep(0.56, 0.86, bloom_field) * 0.55;
    float edge_mask = smoothstep(0.52, 0.61, wash_field) - smoothstep(0.66, 0.77, wash_field);

    float vignette = smoothstep(0.18, 0.82, distance(UV, vec2(0.5))) * vignette_strength;
    vec3 color = paper_color.rgb + vec3(paper_noise) - vec3(vignette);

    // 先铺开柔和的水痕，再在边缘压出略深的“积墨”轮廓，形成宣纸吸墨的层次感。
    color -= wash_tint * wash_mask * wash_strength;
    color -= wash_tint * bloom_mask * bloom_strength;
    color -= vec3(edge_mask * edge_accent_strength);
    color = clamp(color, vec3(0.0), vec3(1.0));
    COLOR = vec4(color, paper_color.a);
}
