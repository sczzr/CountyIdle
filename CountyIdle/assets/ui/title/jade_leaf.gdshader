shader_type canvas_item;

uniform vec4 top_color : source_color = vec4(0.9922, 0.9922, 0.9922, 1.0);
uniform vec4 bottom_color : source_color = vec4(0.7961, 0.8627, 0.8157, 1.0);
uniform vec4 border_color : source_color = vec4(0.0, 0.0, 0.0, 0.08);
uniform float corner_radius = 0.115;
uniform float border_width = 0.018;
uniform float highlight_strength = 0.08;

float rounded_rect(vec2 uv, float radius)
{
    vec2 half_extent = vec2(0.5 - radius);
    vec2 delta = abs(uv - vec2(0.5)) - half_extent;
    float outside = length(max(delta, vec2(0.0))) - radius;
    return 1.0 - smoothstep(0.0, 0.01, outside);
}

void fragment()
{
    float radius = clamp(corner_radius, 0.02, 0.24);
    float border = clamp(border_width, 0.004, radius * 0.9);
    float mask = rounded_rect(UV, radius);
    float inner_mask = rounded_rect(UV, max(radius - border, 0.001));
    float gradient_t = clamp((UV.y * 0.84) + (UV.x * 0.16), 0.0, 1.0);
    vec3 base = mix(top_color.rgb, bottom_color.rgb, gradient_t);
    float top_highlight = (1.0 - smoothstep(0.0, 0.34, UV.y)) * highlight_strength;
    float center_softening = (1.0 - smoothstep(0.0, 0.5, abs(UV.x - 0.5))) * 0.025;
    vec3 fill = clamp(base + vec3(top_highlight + center_softening), 0.0, 1.0);
    vec3 color = mix(border_color.rgb, fill, inner_mask);
    COLOR = vec4(color, mask);
}
