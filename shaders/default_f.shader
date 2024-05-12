#version 330 core
in float shade_f;
in vec2 uv_f;

out vec4 FragColor;

uniform sampler2D texture0;

void main()
{
	vec4 texcolor = texture(texture0, uv_f);
	float shadefb = max(texcolor.a, shade_f);
    FragColor = texcolor * shadefb;
}