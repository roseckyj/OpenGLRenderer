#version 330 core

#define MAX_LIGHTS 10

struct Material {
    vec3 ambientColor;
    vec3 diffuseColor;
    vec3 specularColor;

    sampler2D diffuseTex;
    sampler2D specularTex;
    int useDiffuseTex;
    int useSpecularTex;

    float shininess;
};


struct DirLight {
    vec3 direction;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;

    sampler2D shadowMap;
    mat4 matrix;
};
uniform DirLight dirLight;

struct PointLight {
    int use;

    vec3 position;

    float constant;
    float linear;
    float quadratic;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;

    sampler2D shadowMap;
    mat4 matrix;
};
uniform PointLight pointLights[MAX_LIGHTS];


struct SpotLight{
    int use;

    vec3 position;
    vec3 direction;
    float cutOff;
    float outerCutOff;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;

    float constant;
    float linear;
    float quadratic;

    sampler2D shadowMap;
    mat4 matrix;
};
uniform SpotLight spotLights[MAX_LIGHTS];


uniform Material material;
uniform vec3 viewPos;

out vec4 FragColor;

in vec3 normal;
in vec3 fragPos;
in vec2 texCoord;

// Function prototypes
vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir, float shadow);
vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir, float shadow);
vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir, float shadow);
vec3 GetAmbientColor();
vec3 GetDiffuseColor();
vec3 GetSpecularColor();
float GetShadow(mat4 matrix, sampler2D shadowMap, vec3 normal);


void main()
{
    if (material.useDiffuseTex > 0 && texture(material.diffuseTex, texCoord).w < 0.1) {
        discard;
    }

    vec3 norm = normalize(normal);
    vec3 viewDir = normalize(viewPos - fragPos);

    float dist = length(viewPos - fragPos);


    // Directional light
    float shadowDir = GetShadow(dirLight.matrix, dirLight.shadowMap, norm);
    vec3 result = CalcDirLight(dirLight, norm, viewDir, shadowDir);
    
    // Point lights
    for(int i = 0; i < MAX_LIGHTS; i++) {
        if (pointLights[i].use == 0) break;
        float shadowPoint = GetShadow(pointLights[i].matrix, pointLights[i].shadowMap, norm);
        result += CalcPointLight(pointLights[i], norm, fragPos, viewDir, shadowPoint);
    }
    
    // Spot lights
    for(int i = 0; i < MAX_LIGHTS; i++){
        if (spotLights[i].use == 0) break;
        float shadowSpot = GetShadow(spotLights[i].matrix, spotLights[i].shadowMap, norm);
        result += CalcSpotLight(spotLights[i], norm, fragPos, viewDir, shadowSpot);
    }

    FragColor = mix(vec4(result, 1.0), vec4(172 / 255.0, 225 / 255.0, 235 / 255.0, 1.0), clamp(dist * 0.1f - 4, 0, 1));
    
}

vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir, float shadow)
{
    vec3 lightDir = normalize(light.direction);
    //diffuse shading
    float diff = max(dot(normal, lightDir), 0.0);
    //specular shading
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    //combine results
    vec3 ambient  = light.ambient  * GetAmbientColor();
    vec3 diffuse  = light.diffuse  * diff * GetDiffuseColor() * shadow;
    vec3 specular = light.specular * spec * GetSpecularColor() * shadow;
    return (ambient + diffuse + specular);
}

vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir, float shadow)
{
    vec3 lightDir = normalize(light.position - fragPos);
    //diffuse shading
    float diff = max(dot(normal, lightDir), 0.0);
    //specular shading
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    //attenuation
    float distance    = length(light.position - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance +
    light.quadratic * (distance * distance));
    //combine results
    vec3 ambient  = light.ambient  * GetAmbientColor();
    vec3 diffuse  = light.diffuse  * diff * GetDiffuseColor();
    vec3 specular = light.specular * spec * GetSpecularColor();
    ambient  *= attenuation;
    diffuse  *= attenuation * shadow;
    specular *= attenuation * shadow;
    return (ambient + diffuse + specular);
} 

vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir, float shadow)
{
    //diffuse shading
    vec3 lightDir = normalize(light.position - fragPos);
    float diff = max(dot(normal, lightDir), 0.0);

    //specular shading
    vec3 reflectDir = reflect(lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);

    //attenuation
    float distance    = length(light.position - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance +
    light.quadratic * (distance * distance));

    //spotlight intensity
    float theta     = dot(lightDir, normalize(-light.direction));
    float epsilon   = light.cutOff - light.outerCutOff;
    float intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0);

    //combine results
    vec3 ambient = light.ambient * GetAmbientColor();
    vec3 diffuse = light.diffuse * diff * GetDiffuseColor();
    vec3 specular = light.specular * spec * GetSpecularColor();
    ambient  *= attenuation;
    diffuse  *= attenuation * intensity * shadow;
    specular *= attenuation * intensity * shadow;
    return (ambient + diffuse + specular);
}

vec3 GetAmbientColor()
{
    if (material.useDiffuseTex > 0) {
        return vec3(texture(material.diffuseTex, texCoord)) * material.ambientColor;
    }
    return material.ambientColor;
}

vec3 GetDiffuseColor()
{
    if (material.useDiffuseTex > 0) {
        return vec3(texture(material.diffuseTex, texCoord)) * material.diffuseColor;
    }
    return material.diffuseColor;
}

vec3 GetSpecularColor()
{
    if (material.useSpecularTex > 0) {
        return vec3(texture(material.specularTex, texCoord)) * material.specularColor;
    }
    return material.specularColor;
}

float GetShadowTex(sampler2D shadowMap, vec2 texCoords, float currentDepth) {
    float bias = 0.002;    
    int rad = 2;

    int items = 0;
    float shadow = 0;
    for (int x = -rad; x <= rad; x++) {
        for (int y = -rad; y <= rad; y++) {
            float pcfDepth = texture(shadowMap, (texCoords + vec2(x, y)) / textureSize(shadowMap, 0)).r;
            shadow += (currentDepth - bias > pcfDepth ? 0.0 : 1.0);
            items++;
        }
    }

    return shadow / items;    
}

float GetShadow(mat4 matrix, sampler2D shadowMap, vec3 normal) {
    vec4 pos = vec4(fragPos, 1.0) * matrix;
    vec3 projCoords = pos.xyz / pos.w;
    projCoords = projCoords * 0.5 + 0.5;
    float closestDepth = texture(shadowMap, projCoords.xy).r;
    float currentDepth = projCoords.z;

    vec2 texCoords = projCoords.xy * textureSize(shadowMap, 0);

    float bias = 0.002;

    float pcfDepth;

    pcfDepth = GetShadowTex(shadowMap, vec2(floor(texCoords.x), floor(texCoords.y)), currentDepth);
    float shadowFF = (currentDepth - bias > pcfDepth ? 1.0 : 0.0);

    pcfDepth = GetShadowTex(shadowMap, vec2(floor(texCoords.x), ceil(texCoords.y)), currentDepth);
    float shadowFC = (currentDepth - bias > pcfDepth ? 1.0 : 0.0);

    pcfDepth = GetShadowTex(shadowMap, vec2(ceil(texCoords.x), floor(texCoords.y)), currentDepth);
    float shadowCF = (currentDepth - bias > pcfDepth ? 1.0 : 0.0);

    pcfDepth = GetShadowTex(shadowMap, vec2(ceil(texCoords.x), ceil(texCoords.y)), currentDepth);
    float shadowCC = (currentDepth - bias > pcfDepth ? 1.0 : 0.0);

    float left = mix(shadowFF, shadowFC, texCoords.y - floor(texCoords.y));
    float right = mix(shadowCF, shadowCC, texCoords.y - floor(texCoords.y));

    float shadow = mix(left, right, texCoords.x - floor(texCoords.x));


    if(projCoords.z > 1.0) {
        shadow = 0.0;
    }

    return 1.0 - shadow;
}