'use client';

import { useEffect, useState } from 'react';
import { identityService } from '@/services/api';
import Link from 'next/link';
import { useRouter } from 'next/navigation';

export default function Home() {
  const [fullName, setFullName] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const router = useRouter();

  useEffect(() => {
    const checkAuth = async () => {
      try {
        const token = localStorage.getItem('authToken');
        if (!token || !/^[A-Za-z0-9-_=]+\.[A-Za-z0-9-_=]+\.?[A-Za-z0-9-_.+/=]*$/.test(token)) {
          localStorage.removeItem('authToken');
          setIsLoading(false);
          return;
        }

        const user = await identityService.getCurrentUser();
        if (user) {
          setFullName(user.fullName.replace(/[<>]/g, ''));
        } else {
          localStorage.removeItem('authToken');
        }
      } catch (error) {
        console.error('Error fetching user:', error);
        localStorage.removeItem('authToken');
        router.replace('/auth/login');
      } finally {
        setIsLoading(false);
      }
    };

    checkAuth();
  }, [router]);

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-gray-500">Chargement...</div>
      </div>
    );
  }

  return (
    <div className="grid grid-rows-[20px_1fr_20px] items-center justify-items-center min-h-screen p-8 pb-20 gap-16 sm:p-20 font-[family-name:var(--font-geist-sans)]">
      <main className="flex flex-col gap-8 row-start-2 items-center sm:items-start">
        {fullName ? (
          <h1 className="text-3xl font-bold">Welcome {fullName}!</h1>
        ) : (
          <div className="text-center">
            <h1 className="text-3xl font-bold mb-4">Inscrivez-vous</h1>
            <Link 
              href="/auth/register"
              className="text-blue-600 hover:text-blue-800 underline"
            >
              Créer un compte
            </Link>
          </div>
        )}
      </main>
    </div>
  );
}
