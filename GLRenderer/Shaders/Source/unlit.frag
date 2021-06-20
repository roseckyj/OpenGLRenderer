#version 330 core

struct Material {
    vec3 color;
    sampler2D tex;
    int useTex;
};

uniform Material material;
out vec4 FragColor;
in vec2 texCoord;


void main()
{
    if (material.useTex > 0 && texture(material.tex, texCoord).w < 0.1) {
        discard;
    }

    if (material.useTex > 0) {
        FragColor = vec4(vec3(texture(material.tex, texCoord)) * material.color, 1.0);
    } else {
        FragColor = vec4(material.color, 1.0); 
    }   
}