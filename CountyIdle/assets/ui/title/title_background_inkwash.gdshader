shader_type canvas_item;
render_mode unshaded;

uniform vec4 paper_tint : source_color = vec4(0.9569, 0.9451, 0.9176, 1.0);
uniform vec3 ink_tint = vec3(0.42, 0.42, 0.40);
uniform float wash_opacity = 0.62;
uniform float ink_strength = 1.18;
uniform float center_fade = 0.55;
uniform float flow_speed = 0.035;
uniform float flow_amount = 0.0038;
uniform float flow_breath = 0.028;

void fragment()
{
    // 背景图本身已经是山水水墨稿，这里只做轻量的提墨、单色化、慢速流动与中心留白。
    // 为了控制性能，只保留一张纹理采样，流动感通过低频正弦场微弱扭曲 UV 产生。
    float t = TIME * flow_speed;
    float edge_distance = distance(UV, vec2(0.5));
    float edge_emphasis = smoothstep(center_fade * 0.18, center_fade, edge_distance);
    vec2 flow = vec2(
        sin((UV.y * 4.3) + t) + sin((UV.x * 8.1) - (t * 1.37)),
        cos((UV.x * 3.9) - (t * 0.83)) + sin((UV.y * 7.2) + (t * 1.11))
    ) * 0.5;
    vec2 warped_uv = clamp(UV + (flow * flow_amount * mix(0.22, 1.0, edge_emphasis)), vec2(0.0), vec2(1.0));

    vec4 tex = texture(TEXTURE, warped_uv);
    float gray = dot(tex.rgb, vec3(0.299, 0.587, 0.114));
    float darkness = clamp(1.0 - gray, 0.0, 1.0);
    float ink = pow(darkness, ink_strength);

    float alpha = smoothstep(0.025, 0.42, ink) * mix(0.42, 1.0, edge_emphasis) * wash_opacity;
    alpha *= 1.0 + (sin((UV.y * 3.4) + (t * 0.62)) * flow_breath * edge_emphasis);

    vec3 color = mix(paper_tint.rgb, ink_tint, clamp(ink * 1.08, 0.0, 1.0));
    COLOR = vec4(color, alpha);
}
