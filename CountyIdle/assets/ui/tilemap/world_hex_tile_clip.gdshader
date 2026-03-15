shader_type canvas_item;

uniform float hex_half_width : hint_range(0.6, 1.0) = 0.8660254;
uniform float edge_feather : hint_range(0.0, 0.08) = 0.01;
uniform float edge_contract : hint_range(0.0, 0.12) = 0.05;
uniform float edge_contract_width : hint_range(0.0, 0.12) = 0.06;

void fragment() {
    vec2 p = UV * 2.0 - vec2(1.0);
    float ax = abs(p.x);
    float ay = abs(p.y);
    float edge = ay - (1.0 - (ax * 0.57735026919));
    edge = max(edge, ax - hex_half_width);
    float alpha = 1.0 - smoothstep(-edge_feather, edge_feather, edge);

    float edge_mask = smoothstep(-edge_contract_width, 0.0, edge);
    vec2 center = vec2(0.5);
    vec2 dir = UV - center;
    float contract = mix(1.0, 1.0 - edge_contract, edge_mask);
    vec2 sample_uv = center + (dir * contract);
    vec4 color = texture(TEXTURE, sample_uv) * COLOR;

    COLOR = vec4(color.rgb, color.a * alpha);
}
