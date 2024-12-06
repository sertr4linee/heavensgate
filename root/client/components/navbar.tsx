'use client';

import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { useEffect, useState, useCallback } from 'react';
import identityService from '@/services/identity-service';

export default function Navbar() {
    const router = useRouter();
    const [isAuthenticated, setIsAuthenticated] = useState(false);
    const [userName, setUserName] = useState<string | null>(null);
    const [isLoading, setIsLoading] = useState(true);

    const checkAuth = useCallback(async () => {
        try {
            const token = localStorage.getItem('authToken');
            if (!token) {
                setIsAuthenticated(false);
                setUserName(null);
                return;
            }

            const user = await identityService.getCurrentUser();
            setIsAuthenticated(!!user);
            setUserName(user?.fullName || null);
        } catch (error) {
            setIsAuthenticated(false);
            setUserName(null);
        } finally {
            setIsLoading(false);
        }
    }, []);

    useEffect(() => {
        let mounted = true;

        const init = async () => {
            if (mounted) {
                await checkAuth();
            }
        };

        init();

        return () => {
            mounted = false;
        };
    }, [checkAuth]);

    const handleLogout = async () => {
        try {
            await identityService.logout(router);
            setIsAuthenticated(false);
            setUserName(null);
            router.push('/auth/login');
        } catch (error) {
            console.error('Logout failed:', error);
        }
    };

    if (isLoading) {
        return null; // ou un loader si vous préférez
    }

    return (
        <nav className="bg-[#6897bb]/10 border-b border-[#66cdaa]/20">
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                <div className="flex justify-between h-16">
                    <div className="flex">
                        <div className="flex-shrink-0 flex items-center">
                            <Link href="/" className="text-xl font-bold text-[#6897bb]">
                                Dashboard
                            </Link>
                        </div>
                        <div className="hidden sm:ml-6 sm:flex sm:space-x-8">
                            <Link href="/" className="inline-flex items-center px-1 pt-1 text-sm font-medium text-foreground">
                                Home
                            </Link>
                            <Link href="/analytics" className="inline-flex items-center px-1 pt-1 text-sm font-medium text-muted-foreground hover:text-[#b8aff5]">
                                Analytics
                            </Link>
                        </div>
                    </div>
                    <div className="flex items-center">
                        {isAuthenticated ? (
                            <div className="flex items-center space-x-4">
                                <Link 
                                    href="/profile"
                                    className="text-sm text-muted-foreground hover:text-[#b8aff5]"
                                >
                                    Welcome, {userName}
                                </Link>
                                <button
                                    onClick={handleLogout}
                                    className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-[#6897bb] hover:bg-[#b8aff5]/80 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-[#b8aff5]"
                                >
                                    Logout
                                </button>
                            </div>
                        ) : (
                            <div className="space-x-4">
                                <Link
                                    href="/auth/login"
                                    className="inline-flex items-center px-4 py-2 border border-[#6897bb] text-sm font-medium rounded-md text-[#6897bb] bg-transparent hover:bg-[#b8aff5]/10 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-[#b8aff5]"
                                >
                                    Login
                                </Link>
                                <Link
                                    href="/auth/register"
                                    className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-[#6897bb] hover:bg-[#b8aff5]/80 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-[#b8aff5]"
                                >
                                    Register
                                </Link>
                            </div>
                        )}
                    </div>
                </div>
            </div>
        </nav>
    );
} 