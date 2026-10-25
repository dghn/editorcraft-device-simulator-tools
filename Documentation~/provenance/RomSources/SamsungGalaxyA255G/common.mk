#
# Copyright (C) The LineageOS Project
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#      http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

# Inherit from generic products, most specific first
$(call inherit-product, $(SRC_TARGET_DIR)/product/core_64_bit_only.mk)
$(call inherit-product, $(SRC_TARGET_DIR)/product/full_base_telephony.mk)
$(call inherit-product, $(SRC_TARGET_DIR)/product/non_ab_device.mk)

# Inherit proprietary files
$(call inherit-product, vendor/samsung/s5e8825-common/s5e8825-common-vendor.mk)

# Enable project quotas and casefolding for emulated storage without sdcardfs
$(call inherit-product, $(SRC_TARGET_DIR)/product/emulated_storage.mk)

# Inherit Samsung SLSI Linaro configuration
$(call inherit-product, hardware/samsung_slsi-linaro/config/config.mk)

COMMON_PATH := device/samsung/s5e8825-common

# Audio
PRODUCT_PACKAGES += \
    android.hardware.audio@7.0-impl \
    android.hardware.audio.effect@7.0-impl \
    android.hardware.audio.service:64 \
    android.hardware.bluetooth.audio-impl \
    audio.bluetooth.default \
    audio.primary.default \
    audio.r_submix.default \
    audio.usbv2.default

TARGET_EXCLUDES_AUDIOFX := true

$(call soong_config_set,android_hardware_audio,run_64bit,true)

# Audio - Configuration
PRODUCT_PACKAGES += \
    audio_effects.xml \
    audio_policy_configuration.xml \
    usbv2_audio_policy_configuration.xml

PRODUCT_COPY_FILES += \
    frameworks/av/services/audiopolicy/config/audio_policy_volumes.xml:$(TARGET_COPY_OUT_VENDOR)/etc/audio_policy_volumes.xml \
    frameworks/av/services/audiopolicy/config/bluetooth_audio_policy_configuration_7_0.xml:$(TARGET_COPY_OUT_VENDOR)/etc/bluetooth_audio_policy_configuration_7_0.xml \
    frameworks/av/services/audiopolicy/config/r_submix_audio_policy_configuration.xml:$(TARGET_COPY_OUT_VENDOR)/etc/r_submix_audio_policy_configuration.xml \
    frameworks/av/services/audiopolicy/config/default_volume_tables.xml:$(TARGET_COPY_OUT_VENDOR)/etc/default_volume_tables.xml \
    frameworks/av/services/audiopolicy/enginedefault/config/example/phone/audio_policy_engine_configuration.xml:$(TARGET_COPY_OUT_VENDOR)/etc/audio_policy_engine_configuration.xml \
    frameworks/av/services/audiopolicy/enginedefault/config/example/phone/audio_policy_engine_default_stream_volumes.xml:$(TARGET_COPY_OUT_VENDOR)/etc/audio_policy_engine_default_stream_volumes.xml \
    frameworks/av/services/audiopolicy/enginedefault/config/example/phone/audio_policy_engine_product_strategies.xml:$(TARGET_COPY_OUT_VENDOR)/etc/audio_policy_engine_product_strategies.xml \
    frameworks/av/services/audiopolicy/enginedefault/config/example/phone/audio_policy_engine_stream_volumes.xml:$(TARGET_COPY_OUT_VENDOR)/etc/audio_policy_engine_stream_volumes.xml

# Audio - Init
PRODUCT_PACKAGES += init.s5e8825.audio.rc

# Bluetooth
PRODUCT_PACKAGES += \
    android.hardware.bluetooth@1.0-impl:64 \
    android.hardware.bluetooth@1.0-service \
    libbt-vendor:64

PRODUCT_COPY_FILES += \
    hardware/samsung_slsi/libbt/conf/bt_did.conf:$(TARGET_COPY_OUT_VENDOR)/etc/bluetooth/bt_did.conf \
    hardware/samsung_slsi/libbt/conf/bt_vendor.conf:$(TARGET_COPY_OUT_VENDOR)/etc/bluetooth/bt_vendor.conf

# Bluetooth - Init
PRODUCT_PACKAGES += init.s5e8825.bluetooth.rc

# Camera
PRODUCT_PACKAGES += android.hardware.camera.provider-service.samsung

$(call soong_config_set,samsungCameraVars,extra_ids,60)
$(call soong_config_set,samsungCameraVars,needs_sec_reserved_field,true)
$(call soong_config_set,samsungCameraVars,usage_64bit,true)

# Camera - Init
PRODUCT_PACKAGES += init.s5e8825.camera.rc

# Camera - Shims
PRODUCT_PACKAGES += libvpl

# Charger
PRODUCT_PACKAGES += charger_res_images_vendor

# Codec2
PRODUCT_PACKAGES += \
    samsung.hardware.media.c2@1.2-service \
    libExynosC2H264Dec \
    libExynosC2H264Enc \
    libExynosC2HevcDec \
    libExynosC2HevcEnc \
    libExynosC2Vp8Dec \
    libExynosC2Vp8Enc \
    codec2.vendor.base.policy \
    codec2.vendor.ext.policy

# ConfigStore
PRODUCT_PACKAGES += disable_configstore

# Display
PRODUCT_PACKAGES += \
    android.hardware.graphics.allocator@4.0-service \
    android.hardware.graphics.mapper@4.0-impl \
    android.hardware.composer.hwc3-service.slsi

PRODUCT_AAPT_CONFIG := normal
PRODUCT_AAPT_PREF_CONFIG := 450dpi
PRODUCT_AAPT_PREBUILT_DPI := xxxhdpi xxhdpi xhdpi hdpi
TARGET_SCREEN_DENSITY := 450

# Dynamic Partitions
PRODUCT_USE_DYNAMIC_PARTITIONS := true

# DRM
PRODUCT_PACKAGES += android.hardware.drm-service.clearkey

# fastbootd
PRODUCT_PACKAGES += fastbootd

# Fingerprint
PRODUCT_PACKAGES += android.hardware.biometrics.fingerprint-service.s5e8825

# Fingerprint - Init
PRODUCT_PACKAGES += init.s5e8825.fingerprint.rc

# Gatekeeper
PRODUCT_PACKAGES += \
    android.hardware.gatekeeper@1.0-impl:64 \
    android.hardware.gatekeeper@1.0-service

# GPS - Init
PRODUCT_PACKAGES += init.s5e8825.gps.rc

# Graphics
$(call soong_config_set,exynos_hwc,force_client_video,true)

# Health
PRODUCT_PACKAGES += \
    android.hardware.health-service.s5e8825 \
    android.hardware.health-service.s5e8825-recovery

# Init
PRODUCT_PACKAGES += \
    fstab.s5e8825 \
    init.s5e8825.rc \
    ueventd.s5e8825.rc

PRODUCT_COPY_FILES += $(COMMON_PATH)/configs/init/fstab.s5e8825:$(TARGET_COPY_OUT_VENDOR_RAMDISK)/fstab.s5e8825

# Kernel
PRODUCT_OTA_ENFORCE_VINTF_KERNEL_REQUIREMENTS := true
PRODUCT_SET_DEBUGFS_RESTRICTIONS := true
PRODUCT_ENABLE_UFFD_GC := true

# Keymaster
PRODUCT_PACKAGES += \
    libkeymaster_messages.vendor \
    libkeymaster_portable.vendor

# Keymint
PRODUCT_PACKAGES += \
    android.hardware.security.keymint-V3-ndk.vendor \
    lib_android_keymaster_keymint_utils.vendor \
    libcppbor_external.vendor \
    libkeymint.vendor

# Kernel Modules
PRODUCT_PACKAGES += toolbox.vendor_ramdisk

# Libinit
$(call soong_config_set,libinit,vendor_init_lib,//$(COMMON_PATH)/configs/init/libinit:libinit_s5e8825)

# Lineage Health
PRODUCT_PACKAGES += vendor.lineage.health-service.default

$(call soong_config_set,lineage_health,charging_control_charging_path,/sys/class/power_supply/battery/charging_enabled)
$(call soong_config_set,lineage_health,fast_charge_node,/sys/class/sec/switch/afc_disable)
$(call soong_config_set,lineage_health,fast_charge_value_none,1)
$(call soong_config_set,lineage_health,fast_charge_value_fast_charge,0)

# Linker
PRODUCT_PACKAGES += public.libraries.txt

# Live Display
ifneq ($(TARGET_DEVICE),m33x)
PRODUCT_PACKAGES += vendor.lineage.livedisplay-service.samsung-exynos
endif

# Log Tag
include $(COMMON_PATH)/configs/vendor_logtag.mk

# Media
PRODUCT_PACKAGES += \
    media_codecs_c2.xml \
    media_codecs_performance_c2.xml

# Memtrack
PRODUCT_PACKAGES += android.hardware.memtrack-service.samsung-mali

# NFC
PRODUCT_PACKAGES += \
    com.android.nfc_extras \
    libnfc-nci \
    libnfc_nci_jni \
    Tag

# Overlays
DEVICE_PACKAGE_OVERLAYS += $(COMMON_PATH)/overlay

# Permissions
PRODUCT_COPY_FILES += \
    frameworks/native/data/etc/android.hardware.audio.pro.xml:$(TARGET_COPY_OUT_VENDOR)/etc/permissions/android.hardware.audio.pro.xml \
    frameworks/native/data/etc/android.hardware.camera.xml:$(TARGET_COPY_OUT_VENDOR)/etc/permissions/android.hardware.camera.xml \
    frameworks/native/data/etc/android.hardware.camera.ar.xml:$(TARGET_COPY_OUT_VENDOR)/etc/permissions/android.hardware.camera.ar.xml \
    frameworks/native/data/etc/android.hardware.camera.autofocus.xml:$(TARGET_COPY_OUT_VENDOR)/etc/permissions/android.hardware.camera.autofocus.xml \
    frameworks/native/data/etc/android.hardware.camera.flash-autofocus.xml:$(TARGET_COPY_OUT_VENDOR)/etc/permissions/android.hardware.camera.flash-autofocus.xml \
    frameworks/native/data/etc/android.hardware.nfc.ese.xml:$(TARGET_COPY_OUT_VENDOR)/etc/permissions/android.hardware.nfc.ese.xml \
    frameworks/native/data/etc/android.hardware.nfc.hcef.xml:$(TARGET_COPY_OUT_VENDOR)/etc/permissions/android.hardware.nfc.hcef.xml \
    frameworks/native/data/etc/android.hardware.nfc.uicc.xml:$(TARGET_COPY_OUT_VENDOR)/etc/permissions/android.hardware.nfc.uicc.xml \
    frameworks/native/data/etc/android.hardware.opengles.aep.xml:$(TARGET_COPY_OUT_VENDOR)/etc/permissions/android.hardware.opengles.aep.xml \
    frameworks/native/data/etc/android.hardware.touchscreen.multitouch.jazzhand.xml:$(TARGET_COPY_OUT_VENDOR)/etc/permissions/android.hardware.touchscreen.multitouch.jazzhand.xml \
    frameworks/native/data/etc/android.software.app_widgets.xml:$(TARGET_COPY_OUT_VENDOR)/etc/permissions/android.software.app_widgets.xml \
    frameworks/native/data/etc/android.software.freeform_window_management.xml:$(TARGET_COPY_OUT_VENDOR)/etc/permissions/android.software.freeform_window_management.xml \
    frameworks/native/data/etc/android.software.midi.xml:$(TARGET_COPY_OUT_VENDOR)/etc/permissions/android.software.midi.xml \
    frameworks/native/data/etc/android.software.picture_in_picture.xml:$(TARGET_COPY_OUT_VENDOR)/etc/permissions/android.software.picture_in_picture.xml \
    frameworks/native/data/etc/com.nxp.mifare.xml:$(TARGET_COPY_OUT_VENDOR)/etc/permissions/com.nxp.mifare.xml

PRODUCT_PACKAGES += \
    android.hardware.audio.low_latency.prebuilt.xml \
    android.hardware.bluetooth.prebuilt.xml \
    android.hardware.bluetooth_le.prebuilt.xml \
    android.hardware.camera.front.prebuilt.xml \
    android.hardware.camera.full.prebuilt.xml \
    android.hardware.camera.raw.prebuilt.xml \
    android.hardware.ethernet.prebuilt.xml \
    android.hardware.fingerprint.prebuilt.xml \
    android.hardware.location.gps.prebuilt.xml \
    android.hardware.nfc.prebuilt.xml \
    android.hardware.nfc.hce.prebuilt.xml \
    android.hardware.sensor.accelerometer.prebuilt.xml \
    android.hardware.sensor.gyroscope.prebuilt.xml \
    android.hardware.sensor.light.prebuilt.xml \
    android.hardware.sensor.stepcounter.prebuilt.xml \
    android.hardware.sensor.stepdetector.prebuilt.xml \
    android.hardware.telephony.gsm.prebuilt.xml \
    android.hardware.usb.accessory.prebuilt.xml \
    android.hardware.usb.host.prebuilt.xml \
    android.hardware.vulkan.compute-0.prebuilt.xml \
    android.hardware.vulkan.level-1.prebuilt.xml \
    android.hardware.vulkan.version-1_3.prebuilt.xml \
    android.hardware.wifi.direct.prebuilt.xml \
    android.hardware.wifi.passpoint.prebuilt.xml \
    android.hardware.wifi.prebuilt.xml \
    android.software.ipsec_tunnels.prebuilt.xml \
    android.software.opengles.deqp.level-2022-03-01.prebuilt.xml \
    android.software.sip.voip.prebuilt.xml \
    android.software.vulkan.deqp.level-2022-03-01.prebuilt.xml

# Power
PRODUCT_PACKAGES += android.hardware.power-service.pixel-libperfmgr

# Power - Powerhint
PRODUCT_PACKAGES += powerhint.json

# Recovery
PRODUCT_PACKAGES += init.s5e8825.recovery.rc

# RIL
PRODUCT_PACKAGES += \
    cbd \
    secril_config_svc \
    sehradiomanager

$(call soong_config_set,cbd,protocol,sipc)

# RIL - Radio Configuration
PRODUCT_PACKAGES += sehradiomanager.conf

# RIL - Init
PRODUCT_PACKAGES += init.s5e8825.ril.rc

# Samsung DAP
PRODUCT_PACKAGES += SamsungDAP-custom

# Samsung Doze
PRODUCT_PACKAGES += SamsungDoze

# Sensors
PRODUCT_PACKAGES += android.hardware.sensors-service.samsung-multihal

# Sensors - Init
PRODUCT_PACKAGES += init.s5e8825.sensors.rc

# Soong namespaces
PRODUCT_SOONG_NAMESPACES += \
    $(COMMON_PATH) \
    bootable/deprecated-ota \
    hardware/google/interfaces \
    hardware/google/pixel \
    hardware/samsung \
    hardware/samsung_slsi-linaro/exynos/cpboot_v3 \
    hardware/samsung_slsi/libbt

# SpeakerFX
ifneq ($(wildcard packages/apps/SpeakerFX),)
PRODUCT_PACKAGES += SpeakerFX
endif

# Thermal
PRODUCT_PACKAGES += btcon.json

# Touch HAL
PRODUCT_PACKAGES += vendor.lineage.touch-service.samsung

# Updater
AB_OTA_UPDATER := false

# USB
PRODUCT_PACKAGES += \
    android.hardware.usb-service.samsung \
    android.hardware.usb.gadget-service.samsung

$(call soong_config_set,samsungUsbGadgetVars,gadget_name,13200000.dwc3)

# USB - Init
PRODUCT_PACKAGES += init.s5e8825.usb.rc

# Wi-Fi
PRODUCT_PACKAGES += \
    android.hardware.wifi-service \
    hostapd \
    wpa_supplicant

PRODUCT_CFI_INCLUDE_PATHS += hardware/samsung_slsi/scsc_wifibt/wpa_supplicant_lib

# Wi-Fi - Configuration
PRODUCT_PACKAGES += \
    p2p_supplicant_overlay.conf \
    wpa_supplicant.conf \
    wpa_supplicant_overlay.conf

# Wi-Fi - Init
PRODUCT_PACKAGES += init.s5e8825.wifi.rc

# Vibrator
PRODUCT_PACKAGES += android.hardware.vibrator-service.samsung
