import { createClient, SupabaseClient } from '@supabase/supabase-js';

export const SUPABASE_TARGET_URL = 'https://nyczzsdkzdscyffbpdun.supabase.co';

let serverSupabaseClient: SupabaseClient | null = null;

/**
 * Lazy initialization of Supabase Server-side Client.
 * Uses SUPABASE_SERVICE_ROLE_KEY or SUPABASE_ANON_KEY.
 * Never throws at load time.
 */
export function getSupabaseAdmin(): SupabaseClient | null {
  if (serverSupabaseClient) {
    return serverSupabaseClient;
  }

  const url = process.env.SUPABASE_URL || SUPABASE_TARGET_URL;
  const key = process.env.SUPABASE_SERVICE_ROLE_KEY || process.env.SUPABASE_ANON_KEY;

  if (!key) {
    return null;
  }

  try {
    serverSupabaseClient = createClient(url, key, {
      auth: {
        persistSession: false,
        autoRefreshToken: false,
      },
    });
    return serverSupabaseClient;
  } catch (error) {
    console.error('[Supabase] Failed to initialize server client:', error);
    return null;
  }
}

/**
 * Test connectivity with Supabase project endpoint
 */
export async function testSupabaseConnection(): Promise<{
  connected: boolean;
  projectUrl: string;
  hasKey: boolean;
  keyType: 'service_role' | 'anon' | 'none';
  latencyMs: number;
  message: string;
  details?: any;
}> {
  const url = process.env.SUPABASE_URL || SUPABASE_TARGET_URL;
  const key = process.env.SUPABASE_SERVICE_ROLE_KEY || process.env.SUPABASE_ANON_KEY;
  const keyType = process.env.SUPABASE_SERVICE_ROLE_KEY
    ? 'service_role'
    : process.env.SUPABASE_ANON_KEY
    ? 'anon'
    : 'none';

  const startTime = Date.now();

  try {
    // Attempt standard REST health check ping to the Supabase endpoint
    const response = await fetch(`${url}/rest/v1/`, {
      method: 'GET',
      headers: key ? { apikey: key, Authorization: `Bearer ${key}` } : {},
    });

    const latencyMs = Date.now() - startTime;
    const isOk = response.status === 200 || response.status === 401 || response.status === 404;

    if (key && response.status === 200) {
      return {
        connected: true,
        projectUrl: url,
        hasKey: true,
        keyType,
        latencyMs,
        message: 'اتصال کامل به پایگاه داده Supabase با موفقیت برقرار شد.',
      };
    } else if (key && response.status === 401) {
      return {
        connected: false,
        projectUrl: url,
        hasKey: true,
        keyType,
        latencyMs,
        message: 'کلید احراز هویت Supabase نامعتبر است (Unauthorized). لطفا کلید را بررسی کنید.',
      };
    } else {
      return {
        connected: isOk,
        projectUrl: url,
        hasKey: Boolean(key),
        keyType,
        latencyMs,
        message: key
          ? `پاسخ از پروژه با وضعیت ${response.status} دریافت شد.`
          : 'سرور Supabase در دسترس است. برای خواندن و نوشتن مستقیم داده‌ها، کلید Service Role یا Anon را در تنظیمات وارد کنید.',
      };
    }
  } catch (error: any) {
    const latencyMs = Date.now() - startTime;
    return {
      connected: false,
      projectUrl: url,
      hasKey: Boolean(key),
      keyType,
      latencyMs,
      message: `خطا در ارتباط با سرور Supabase: ${error?.message || 'ارتباط برقرار نشد'}`,
    };
  }
}
